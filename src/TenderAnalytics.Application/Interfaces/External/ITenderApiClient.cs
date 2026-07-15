using TenderAnalytics.Application.DTOs.External.Feed;
using TenderAnalytics.Application.DTOs.External.Tender;

namespace TenderAnalytics.Application.Interfaces.External;

public interface ITenderApiClient
{
    Task<FeedResponse> GetFeedPageAsync(
        string? pageUri = null,
        CancellationToken cancellationToken = default);

    Task<TenderResponse> GetTenderAsync(
        string tenderId,
        CancellationToken cancellationToken = default);
}