using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class TenderResponse
{
    [JsonPropertyName("data")]
    public TenderDto? Data { get; init; }
}