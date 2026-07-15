using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class SupplierDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("identifier")]
    public OrganizationIdentifierDto? Identifier { get; init; }
}