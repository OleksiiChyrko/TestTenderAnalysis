using Microsoft.EntityFrameworkCore;
using TenderAnalytics.Application.DTOs.Analytics;
using TenderAnalytics.Application.Interfaces.Services;
using TenderAnalytics.Infrastructure.Persistence;

namespace TenderAnalytics.Infrastructure.Analytics;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _dbContext;

    public AnalyticsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BudgetSavingsDto> GetBudgetSavingsAsync(
        CancellationToken cancellationToken = default)
    {
        var expectedAmount = await _dbContext.Tenders
            .AsNoTracking()
            .SumAsync(
                tender => (decimal?)tender.ExpectedAmount,
                cancellationToken)
            ?? 0m;

        var contractAmount = await _dbContext.Contracts
            .AsNoTracking()
            .SumAsync(
                contract => (decimal?)contract.Amount,
                cancellationToken)
            ?? 0m;

        return new BudgetSavingsDto
        {
            ExpectedAmount = expectedAmount,
            ContractAmount = contractAmount,
            SavingsAmount = expectedAmount - contractAmount,
            Currency = "UAH"
        };
    }

    public async Task<IReadOnlyCollection<TopProcurerDto>>
        GetTopProcurersAsync(
            int limit,
            CancellationToken cancellationToken = default)
    {
        ValidateLimit(limit);

        return await _dbContext.Tenders
            .AsNoTracking()
            .SelectMany(
                tender => tender.Contracts,
                (tender, contract) => new
                {
                    tender.ProcuringEntityIdentifier,
                    tender.ProcuringEntityName,
                    TenderId = tender.Id,
                    ContractAmount = contract.Amount
                })
            .GroupBy(item => new
            {
                item.ProcuringEntityIdentifier,
                item.ProcuringEntityName
            })
            .Select(group => new TopProcurerDto
            {
                Identifier =
                    group.Key.ProcuringEntityIdentifier,

                Name =
                    group.Key.ProcuringEntityName,

                TotalContractAmount =
                    group.Sum(item => item.ContractAmount),

                TendersCount =
                    group.Select(item => item.TenderId)
                        .Distinct()
                        .Count()
            })
            .OrderByDescending(item =>
                item.TotalContractAmount)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TopSupplierDto>>
        GetTopSuppliersAsync(
            int limit,
            CancellationToken cancellationToken = default)
    {
        ValidateLimit(limit);

        return await _dbContext.ContractSuppliers
            .AsNoTracking()
            .GroupBy(link => new
            {
                link.Supplier.Identifier,
                link.Supplier.Name
            })
            .Select(group => new TopSupplierDto
            {
                Identifier =
                    group.Key.Identifier,

                Name =
                    group.Key.Name,

                TotalContractAmount =
                    group.Sum(link =>
                        link.Contract.Amount),

                ContractsCount =
                    group.Select(link =>
                            link.ContractId)
                        .Distinct()
                        .Count()
            })
            .OrderByDescending(item =>
                item.TotalContractAmount)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private static void ValidateLimit(int limit)
    {
        if (limit is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                "Limit must be between 1 and 100.");
        }
    }
}