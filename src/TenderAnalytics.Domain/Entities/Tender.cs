using System.Diagnostics.Contracts;

namespace TenderAnalytics.Domain.Entities;

public sealed class Tender
{
    public string Id { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTimeOffset DateCreated { get; set; }

    public decimal ExpectedAmount { get; set; }

    public string Currency { get; set; } = null!;

    public string CpvCode { get; set; } = null!;

    public string? ProcuringEntityIdentifier { get; set; }

    public string ProcuringEntityName { get; set; } = null!;

    public DateTimeOffset ImportedAt { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}