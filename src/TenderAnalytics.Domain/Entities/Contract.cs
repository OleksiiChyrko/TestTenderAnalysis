namespace TenderAnalytics.Domain.Entities;

public sealed class Contract
{
    public string Id { get; set; } = null!;

    public string TenderId { get; set; } = null!;

    public string? AwardId { get; set; }

    public string? Status { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public Tender Tender { get; set; } = null!;

    public ICollection<ContractSupplier> ContractSuppliers { get; set; }
        = new List<ContractSupplier>();
}