namespace TenderAnalytics.Api.Models;

public sealed class ApiErrorResponse
{
    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? TraceId { get; init; }
}