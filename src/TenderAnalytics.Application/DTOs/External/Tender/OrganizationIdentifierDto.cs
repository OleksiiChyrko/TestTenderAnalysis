using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class OrganizationIdentifierDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("scheme")]
    public string? Scheme { get; init; }
}