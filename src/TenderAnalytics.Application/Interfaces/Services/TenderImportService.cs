using System.Collections.Concurrent;
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

    public TenderImportService(
        ITenderApiClient apiClient,
        ITenderMapper mapper,
        ITenderRepository repository)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<bool> ImportTenderAsync(
        string tenderId,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetTenderAsync(
            tenderId,
            cancellationToken);

        var source = response.Data;

        var request = CreateDefaultImportRequest();

        if (source is null ||
            !IsMatchingTender(source, request))
        {
            return false;
        }

        var tender = _mapper.Map(source);

        await _repository.UpsertAsync(
            tender,
            cancellationToken);

        return true;
    }

    public async Task<ImportResult> ImportFeedAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

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
                .Where(x => IsFeedCandidate(x, request))
                .ToList();

            result.CandidatesCount += candidates.Count;

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
                            downloadErrors.Add(
                                $"{candidate.Id}: response data is empty.");

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
                        downloadErrors.Add(
                            $"{candidate.Id}: {exception.Message}");
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
                        continue;
                    }

                    var tender =
                        _mapper.Map(downloadedTender.Data);

                    await _repository.UpsertAsync(
                        tender,
                        cancellationToken);

                    result.ImportedCount++;
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
                }
            }

            nextPageUri = page.NextPage?.Uri;

            if (page.Data.Count == 0 ||
                string.IsNullOrWhiteSpace(nextPageUri))
            {
                break;
            }
        }

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
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxPages),
                "MaxPages must be greater than zero.");
        }

        if (request.MaxConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxConcurrency),
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

        return tender.Items.Any(x =>
            string.Equals(
                x.Classification?.Id,
                RequiredCpvCode,
                StringComparison.OrdinalIgnoreCase));
    }
}