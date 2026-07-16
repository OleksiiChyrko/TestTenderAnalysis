using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TenderAnalytics.Application.DTOs.External.Feed;
using TenderAnalytics.Application.DTOs.External.Tender;
using TenderAnalytics.Application.DTOs.Import;
using TenderAnalytics.Application.Interfaces.External;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Application.Interfaces.Repositories;
using TenderAnalytics.Application.Interfaces.Services;

namespace TenderAnalytics.Application.Services;

public sealed class TenderImportService : ITenderImportService
{
    private const string RequiredCpvCode = "09310000-5";
    private const string RequiredStatus = "complete";

    private readonly ITenderApiClient _apiClient;
    private readonly ITenderMapper _mapper;
    private readonly ITenderRepository _repository;
    private readonly ILogger<TenderImportService> _logger;

    public TenderImportService(
        ITenderApiClient apiClient,
        ITenderMapper mapper,
        ITenderRepository repository,
        ILogger<TenderImportService> logger)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> ImportTenderAsync(
        string tenderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenderId);

        _logger.LogInformation(
            "Starting single tender import. TenderId: {TenderId}",
            tenderId);

        var response = await _apiClient.GetTenderAsync(
            tenderId,
            cancellationToken);

        var source = response.Data;
        var request = CreateDefaultImportRequest();

        if (source is null)
        {
            _logger.LogWarning(
                "Tender response contains no data. TenderId: {TenderId}",
                tenderId);

            return false;
        }

        if (!IsMatchingTender(source, request))
        {
            _logger.LogInformation(
                "Tender does not match import criteria. TenderId: {TenderId}",
                tenderId);

            return false;
        }

        var tender = _mapper.Map(source);

        await _repository.UpsertAsync(
            tender,
            cancellationToken);

        _logger.LogInformation(
            "Tender import completed successfully. TenderId: {TenderId}",
            tenderId);

        return true;
    }

    public async Task<ImportResult> ImportFeedAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        _logger.LogInformation(
            "Starting feed import. DateFrom: {DateFrom}, DateTo: {DateTo}, MaxPages: {MaxPages}, MaxConcurrency: {MaxConcurrency}",
            request.DateFrom,
            request.DateTo,
            request.MaxPages,
            request.MaxConcurrency);

        var result = new ImportResult();
        string? nextPageUri = null;

        for (var pageNumber = 0;
             pageNumber < request.MaxPages;
             pageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = await _apiClient.GetFeedPageAsync(
                nextPageUri,
                cancellationToken);

            result.PagesProcessed++;
            result.FeedItemsProcessed += page.Data.Count;

            var candidates = page.Data
                .Where(item => IsFeedCandidate(item, request))
                .ToList();

            result.CandidatesCount += candidates.Count;

            _logger.LogInformation(
                "Processed feed page {PageNumber}. FeedItems: {FeedItems}, Candidates: {Candidates}",
                pageNumber + 1,
                page.Data.Count,
                candidates.Count);

            var downloadedTenders =
                new ConcurrentBag<(string Id, TenderDto Data)>();

            var downloadErrors =
                new ConcurrentBag<string>();

            await Parallel.ForEachAsync(
                candidates,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism =
                        request.MaxConcurrency,

                    CancellationToken =
                        cancellationToken
                },
                async (candidate, token) =>
                {
                    try
                    {
                        var response =
                            await _apiClient.GetTenderAsync(
                                candidate.Id,
                                token);

                        if (response.Data is null)
                        {
                            var message =
                                $"{candidate.Id}: response data is empty.";

                            downloadErrors.Add(message);

                            _logger.LogWarning(
                                "Tender response contains no data. TenderId: {TenderId}",
                                candidate.Id);

                            return;
                        }

                        downloadedTenders.Add(
                            (candidate.Id, response.Data));
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        var message =
                            $"{candidate.Id}: {exception.Message}";

                        downloadErrors.Add(message);

                        _logger.LogWarning(
                            exception,
                            "Failed to download tender. TenderId: {TenderId}",
                            candidate.Id);
                    }
                });

            result.FailedCount += downloadErrors.Count;
            result.Errors.AddRange(downloadErrors);

            foreach (var downloadedTender in downloadedTenders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (!IsMatchingTender(
                            downloadedTender.Data,
                            request))
                    {
                        result.SkippedCount++;

                        _logger.LogDebug(
                            "Tender skipped because it does not match full import criteria. TenderId: {TenderId}",
                            downloadedTender.Id);

                        continue;
                    }

                    var tender =
                        _mapper.Map(downloadedTender.Data);

                    await _repository.UpsertAsync(
                        tender,
                        cancellationToken);

                    result.ImportedCount++;

                    _logger.LogInformation(
                        "Tender imported successfully. TenderId: {TenderId}",
                        downloadedTender.Id);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    result.FailedCount++;

                    result.Errors.Add(
                        $"{downloadedTender.Id}: {exception.Message}");

                    _logger.LogWarning(
                        exception,
                        "Failed to map or save tender. TenderId: {TenderId}",
                        downloadedTender.Id);
                }
            }

            nextPageUri = page.NextPage?.Uri;

            if (page.Data.Count == 0 ||
                string.IsNullOrWhiteSpace(nextPageUri))
            {
                _logger.LogInformation(
                    "Feed import stopped because no next page is available.");

                break;
            }
        }

        _logger.LogInformation(
            "Feed import completed. PagesProcessed: {PagesProcessed}, FeedItemsProcessed: {FeedItemsProcessed}, Candidates: {Candidates}, Imported: {Imported}, Skipped: {Skipped}, Failed: {Failed}",
            result.PagesProcessed,
            result.FeedItemsProcessed,
            result.CandidatesCount,
            result.ImportedCount,
            result.SkippedCount,
            result.FailedCount);

        return result;
    }

    private static ImportRequest CreateDefaultImportRequest()
    {
        return new ImportRequest
        {
            DateFrom = new DateTimeOffset(
                2025,
                12,
                1,
                0,
                0,
                0,
                TimeSpan.Zero),

            DateTo = new DateTimeOffset(
                2026,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero),

            MaxPages = 1,
            MaxConcurrency = 1
        };
    }

    private static void ValidateRequest(
        ImportRequest request)
    {
        if (request.MaxPages <= 0)
        {
            throw new ArgumentException(
                "MaxPages must be greater than zero.");
        }

        if (request.MaxConcurrency <= 0)
        {
            throw new ArgumentException(
                "MaxConcurrency must be greater than zero.");
        }

        if (request.DateFrom >= request.DateTo)
        {
            throw new ArgumentException(
                "DateFrom must be earlier than DateTo.");
        }
    }

    private static bool IsFeedCandidate(
        FeedTenderDto tender,
        ImportRequest request)
    {
        if (!string.Equals(
                tender.Status,
                RequiredStatus,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (tender.DateCreated is null)
        {
            return false;
        }

        var createdAt =
            tender.DateCreated.Value.ToUniversalTime();

        return createdAt >= request.DateFrom &&
               createdAt < request.DateTo;
    }

    private static bool IsMatchingTender(
        TenderDto tender,
        ImportRequest request)
    {
        if (!string.Equals(
                tender.Status,
                RequiredStatus,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (tender.DateCreated is null)
        {
            return false;
        }

        var createdAt =
            tender.DateCreated.Value.ToUniversalTime();

        if (createdAt < request.DateFrom ||
            createdAt >= request.DateTo)
        {
            return false;
        }

        return tender.Items.Any(item =>
            string.Equals(
                item.Classification?.Id,
                RequiredCpvCode,
                StringComparison.OrdinalIgnoreCase));
    }
}