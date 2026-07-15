using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Feed;

public sealed class FeedItemDto
{
    [JsonPropertyName("classification")]
    public FeedClassificationDto? Classification { get; init; }
}