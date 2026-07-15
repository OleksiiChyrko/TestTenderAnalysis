using TenderAnalytics.Application.DTOs.Analytics;

namespace TenderAnalytics.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<BudgetSavingsDto> GetBudgetSavingsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TopProcurerDto>> GetTopProcurersAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TopSupplierDto>> GetTopSuppliersAsync(
        int limit,
        CancellationToken cancellationToken = default);
}