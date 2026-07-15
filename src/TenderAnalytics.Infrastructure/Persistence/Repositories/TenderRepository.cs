using Microsoft.EntityFrameworkCore;
using TenderAnalytics.Application.Interfaces.Repositories;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Infrastructure.Persistence.Repositories;

public sealed class TenderRepository : ITenderRepository
{
    private readonly AppDbContext _dbContext;

    public TenderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertAsync(
        Tender tender,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tender);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingTender = await _dbContext.Tenders
    .AsSplitQuery()
    .Include(x => x.Contracts)
        .ThenInclude(x => x.ContractSuppliers)
    .FirstOrDefaultAsync(
        x => x.Id == tender.Id,
        cancellationToken);

        if (existingTender is null)
        {
            await AttachExistingSuppliersAsync(
                tender,
                cancellationToken);

            await _dbContext.Tenders.AddAsync(
                tender,
                cancellationToken);
        }
        else
        {
            UpdateTender(existingTender, tender);

            _dbContext.ContractSuppliers.RemoveRange(
                existingTender.Contracts
                    .SelectMany(x => x.ContractSuppliers));

            _dbContext.Contracts.RemoveRange(
                existingTender.Contracts);

            existingTender.Contracts.Clear();

            foreach (var contract in tender.Contracts)
            {
                contract.TenderId = existingTender.Id;
                contract.Tender = existingTender;

                existingTender.Contracts.Add(contract);
            }

            await AttachExistingSuppliersAsync(
                existingTender,
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task AttachExistingSuppliersAsync(
        Tender tender,
        CancellationToken cancellationToken)
    {
        var links = tender.Contracts
            .SelectMany(x => x.ContractSuppliers)
            .ToList();

        var identifiers = links
            .Select(x => x.Supplier.Identifier)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var existingByIdentifier = await _dbContext.Suppliers
            .Where(x =>
                x.Identifier != null &&
                identifiers.Contains(x.Identifier))
            .ToDictionaryAsync(
                x => x.Identifier!,
                StringComparer.Ordinal,
                cancellationToken);

        var suppliersWithoutIdentifier = links
            .Where(x => string.IsNullOrWhiteSpace(
                x.Supplier.Identifier))
            .Select(x => x.Supplier.NormalizedName)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var existingByNormalizedName = await _dbContext.Suppliers
            .Where(x =>
                x.Identifier == null &&
                suppliersWithoutIdentifier.Contains(
                    x.NormalizedName))
            .ToDictionaryAsync(
                x => x.NormalizedName,
                StringComparer.Ordinal,
                cancellationToken);

        var newSuppliersByIdentifier =
            new Dictionary<string, Supplier>(
                StringComparer.Ordinal);

        var newSuppliersByNormalizedName =
            new Dictionary<string, Supplier>(
                StringComparer.Ordinal);

        foreach (var link in links)
        {
            var mappedSupplier = link.Supplier;

            if (!string.IsNullOrWhiteSpace(
                    mappedSupplier.Identifier))
            {
                if (existingByIdentifier.TryGetValue(
                        mappedSupplier.Identifier,
                        out var existingSupplier))
                {
                    link.Supplier = existingSupplier;
                    link.SupplierId = existingSupplier.Id;
                    continue;
                }

                if (newSuppliersByIdentifier.TryGetValue(
                        mappedSupplier.Identifier,
                        out var pendingSupplier))
                {
                    link.Supplier = pendingSupplier;
                    continue;
                }

                newSuppliersByIdentifier.Add(
                    mappedSupplier.Identifier,
                    mappedSupplier);

                continue;
            }

            if (existingByNormalizedName.TryGetValue(
                    mappedSupplier.NormalizedName,
                    out var existingByName))
            {
                link.Supplier = existingByName;
                link.SupplierId = existingByName.Id;
                continue;
            }

            if (newSuppliersByNormalizedName.TryGetValue(
                    mappedSupplier.NormalizedName,
                    out var pendingByName))
            {
                link.Supplier = pendingByName;
                continue;
            }

            newSuppliersByNormalizedName.Add(
                mappedSupplier.NormalizedName,
                mappedSupplier);
        }
    }

    private static void UpdateTender(
        Tender target,
        Tender source)
    {
        target.Status = source.Status;
        target.DateCreated = source.DateCreated;
        target.ExpectedAmount = source.ExpectedAmount;
        target.Currency = source.Currency;
        target.CpvCode = source.CpvCode;
        target.ProcuringEntityIdentifier =
            source.ProcuringEntityIdentifier;
        target.ProcuringEntityName =
            source.ProcuringEntityName;
        target.ImportedAt = source.ImportedAt;
    }
}