using System.Net.Http.Json;
using TenderAnalytics.Application.DTOs.External.Feed;
using TenderAnalytics.Application.DTOs.External.Tender;
using TenderAnalytics.Application.Interfaces.External;

namespace TenderAnalytics.Infrastructure.External.Clients;

public sealed class TenderApiClient : ITenderApiClient
{
    private const string InitialFeedPath =
        "tenders?descending=1&limit=100&opt_fields=dateCreated,status";

    private readonly HttpClient _httpClient;

    public TenderApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FeedResponse> GetFeedPageAsync(
        string? pageUri = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = string.IsNullOrWhiteSpace(pageUri)
            ? InitialFeedPath
            : pageUri;

        using var response = await _httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FeedResponse>(
            cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "Tender feed response body was empty.");
    }

    public async Task<TenderResponse> GetTenderAsync(
        string tenderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenderId);

        var requestUri = $"tenders/{Uri.EscapeDataString(tenderId)}";

        using var response = await _httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TenderResponse>(
            cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                $"Tender '{tenderId}' response body was empty.");
    }
}