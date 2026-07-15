namespace TenderAnalytics.Application.DTOs.Analytics;

public sealed class BudgetSavingsDto
{
    public decimal ExpectedAmount { get; init; }

    public decimal ContractAmount { get; init; }

    public decimal SavingsAmount { get; init; }

    public string Currency { get; init; } = "UAH";
}