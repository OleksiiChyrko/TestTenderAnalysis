using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class TenderAwardDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("suppliers")]
    public IReadOnlyCollection<SupplierDto> Suppliers { get; init; }
        = Array.Empty<SupplierDto>();
}