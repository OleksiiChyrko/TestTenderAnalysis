using TenderAnalytics.Application.DTOs.External.Tender;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Application.Mapping;

public sealed class TenderMapper : ITenderMapper
{
    private const string ActiveContractStatus = "active";

    public Tender Map(TenderDto source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(source.Id))
        {
            throw new InvalidOperationException("Tender id is missing.");
        }

        if (string.IsNullOrWhiteSpace(source.Status))
        {
            throw new InvalidOperationException(
                $"Tender '{source.Id}' status is missing.");
        }

        if (source.DateCreated is null)
        {
            throw new InvalidOperationException(
                $"Tender '{source.Id}' creation date is missing.");
        }

        if (source.Value?.Amount is null)
        {
            throw new InvalidOperationException(
                $"Tender '{source.Id}' expected amount is missing.");
        }

        if (string.IsNullOrWhiteSpace(source.Value.Currency))
        {
            throw new InvalidOperationException(
                $"Tender '{source.Id}' currency is missing.");
        }

        if (string.IsNullOrWhiteSpace(source.ProcuringEntity?.Name))
        {
            throw new InvalidOperationException(
                $"Tender '{source.Id}' procuring entity name is missing.");
        }

        var cpvCode = source.Items
            .Select(x => x.Classification?.Id)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (string.IsNullOrWhiteSpace(cpvCode))
        {
            throw new InvalidOperationException(
                $"Tender '{source.Id}' CPV code is missing.");
        }

        var tender = new Tender
        {
            Id = source.Id,
            Status = source.Status,
            DateCreated = source.DateCreated.Value.ToUniversalTime(),
            ExpectedAmount = source.Value.Amount.Value,
            Currency = source.Value.Currency,
            CpvCode = cpvCode,
            ProcuringEntityIdentifier =
                source.ProcuringEntity.Identifier?.Id,
            ProcuringEntityName = source.ProcuringEntity.Name,
            ImportedAt = DateTimeOffset.UtcNow
        };

        var awardsById = source.Awards
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(x => x.Id!, StringComparer.Ordinal);

        foreach (var contractDto in source.Contracts)
        {
            if (!string.Equals(
                    contractDto.Status,
                    ActiveContractStatus,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(contractDto.Id) ||
                contractDto.Value?.Amount is null ||
                string.IsNullOrWhiteSpace(contractDto.Value.Currency))
            {
                continue;
            }

            var contract = new Contract
            {
                Id = contractDto.Id,
                TenderId = tender.Id,
                AwardId = contractDto.AwardId,
                Status = contractDto.Status,
                Amount = contractDto.Value.Amount.Value,
                Currency = contractDto.Value.Currency
            };

            MapSuppliers(contract, contractDto, awardsById);

            tender.Contracts.Add(contract);
        }

        return tender;
    }

    private static void MapSuppliers(
        Contract contract,
        TenderContractDto contractDto,
        IReadOnlyDictionary<string, TenderAwardDto> awardsById)
    {
        if (string.IsNullOrWhiteSpace(contractDto.AwardId))
        {
            return;
        }

        if (!awardsById.TryGetValue(
                contractDto.AwardId,
                out var award))
        {
            return;
        }

        foreach (var supplierDto in award.Suppliers)
        {
            if (string.IsNullOrWhiteSpace(supplierDto.Name))
            {
                continue;
            }

            var normalizedName = NormalizeName(supplierDto.Name);

            var supplier = new Supplier
            {
                Identifier = supplierDto.Identifier?.Id,
                Name = supplierDto.Name.Trim(),
                NormalizedName = normalizedName
            };

            contract.ContractSuppliers.Add(new ContractSupplier
            {
                Contract = contract,
                Supplier = supplier
            });
        }
    }

    private static string NormalizeName(string value)
    {
        return string.Join(
                ' ',
                value
                    .Trim()
                    .ToUpperInvariant()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries))
            .Trim();
    }
}