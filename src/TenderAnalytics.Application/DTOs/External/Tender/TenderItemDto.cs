using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class TenderItemDto
{
    [JsonPropertyName("classification")]
    public ClassificationDto? Classification { get; init; }
}