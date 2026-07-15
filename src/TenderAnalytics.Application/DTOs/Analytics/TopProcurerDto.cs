namespace TenderAnalytics.Application.DTOs.Analytics;

public sealed class TopProcurerDto
{
    public string? Identifier { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal TotalContractAmount { get; init; }

    public int TendersCount { get; init; }
}