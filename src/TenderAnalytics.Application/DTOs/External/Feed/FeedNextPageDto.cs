using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Feed;

public sealed class FeedNextPageDto
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("offset")]
    public string Offset { get; init; } = string.Empty;
}