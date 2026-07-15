using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Feed;

public sealed class FeedResponse
{
    [JsonPropertyName("data")]
    public IReadOnlyCollection<FeedTenderDto> Data { get; init; }
        = Array.Empty<FeedTenderDto>();

    [JsonPropertyName("next_page")]
    public FeedNextPageDto? NextPage { get; init; }
}