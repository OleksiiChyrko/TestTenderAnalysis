using System.Text.Json.Serialization;

namespace TenderAnalytics.Application.DTOs.External.Tender;

public sealed class TenderDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("dateCreated")]
    public DateTimeOffset? DateCreated { get; init; }

    [JsonPropertyName("dateModified")]
    public DateTimeOffset? DateModified { get; init; }

    [JsonPropertyName("value")]
    public TenderValueDto? Value { get; init; }

    [JsonPropertyName("procuringEntity")]
    public OrganizationDto? ProcuringEntity { get; init; }

    [JsonPropertyName("items")]
    public IReadOnlyCollection<TenderItemDto> Items { get; init; }
        = Array.Empty<TenderItemDto>();

    [JsonPropertyName("contracts")]
    public IReadOnlyCollection<TenderContractDto> Contracts { get; init; }
        = Array.Empty<TenderContractDto>();

    [JsonPropertyName("awards")]
    public IReadOnlyCollection<TenderAwardDto> Awards { get; init; }
        = Array.Empty<TenderAwardDto>();
}