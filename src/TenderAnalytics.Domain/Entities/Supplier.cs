namespace TenderAnalytics.Domain.Entities;

public sealed class Supplier
{
    public long Id { get; set; }

    public string? Identifier { get; set; }

    public string Name { get; set; } = null!;

    public string NormalizedName { get; set; } = null!;

    public ICollection<ContractSupplier> ContractSuppliers { get; set; }
        = new List<ContractSupplier>();
}