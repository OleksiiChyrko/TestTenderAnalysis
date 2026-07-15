namespace TenderAnalytics.Application.DTOs.Import;

public sealed class ImportRequest
{
    public DateTimeOffset DateFrom { get; init; }

    public DateTimeOffset DateTo { get; init; }

    public int MaxPages { get; init; } = 100;

    public int MaxConcurrency { get; init; } = 8;
}