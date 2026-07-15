using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Feed;

public sealed class FeedClassificationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
}