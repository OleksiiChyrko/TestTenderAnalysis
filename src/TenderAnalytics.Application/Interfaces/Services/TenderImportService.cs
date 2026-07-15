using TenderAnalytics.Application.Interfaces.External;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Application.Interfaces.Repositories;
using TenderAnalytics.Application.Interfaces.Services;

namespace TenderAnalytics.Application.Services;

public sealed class TenderImportService : ITenderImportService
{
    private const string RequiredCpvCode = "09310000-5";
    private const string RequiredStatus = "complete";

    private static readonly DateTimeOffset DateFrom =
        new(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset DateTo =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

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

        if (source is null ||
            !IsMatchingTender(source))
        {
            return false;
        }

        var tender = _mapper.Map(source);

        await _repository.UpsertAsync(
            tender,
            cancellationToken);

        return true;
    }

    private static bool IsMatchingTender(
        DTOs.External.Tender.TenderDto tender)
    {
        if (!string.Equals(
                tender.Status,
                RequiredStatus,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (tender.DateCreated is null ||
            tender.DateCreated < DateFrom ||
            tender.DateCreated >= DateTo)
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