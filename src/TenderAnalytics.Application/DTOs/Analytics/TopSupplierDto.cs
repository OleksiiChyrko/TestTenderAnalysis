namespace TenderAnalytics.Application.DTOs.Analytics;

public sealed class TopSupplierDto
{
    public string? Identifier { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal TotalContractAmount { get; init; }

    public int ContractsCount { get; init; }
}