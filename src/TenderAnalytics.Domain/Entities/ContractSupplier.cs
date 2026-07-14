namespace TenderAnalytics.Domain.Entities;

public sealed class ContractSupplier
{
    public string ContractId { get; set; } = null!;

    public long SupplierId { get; set; }

    public Contract Contract { get; set; } = null!;

    public Supplier Supplier { get; set; } = null!;
}