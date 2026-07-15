using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Feed;

public sealed class FeedTenderDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("dateModified")]
    public DateTimeOffset DateModified { get; init; }

    [JsonPropertyName("dateCreated")]
    public DateTimeOffset? DateCreated { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("items")]
    public IReadOnlyCollection<FeedItemDto> Items { get; init; }
        = Array.Empty<FeedItemDto>();
}