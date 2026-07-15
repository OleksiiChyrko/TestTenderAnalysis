using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class TenderContractDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("awardID")]
    public string? AwardId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("value")]
    public TenderValueDto? Value { get; init; }
}