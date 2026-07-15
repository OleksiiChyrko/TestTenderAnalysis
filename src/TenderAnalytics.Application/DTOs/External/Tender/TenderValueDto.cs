using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class TenderValueDto
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }
}