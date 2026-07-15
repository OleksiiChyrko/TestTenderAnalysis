namespace TenderAnalytics.Application.Interfaces.Services;

public interface ITenderImportService
{
    Task<bool> ImportTenderAsync(
        string tenderId,
        CancellationToken cancellationToken = default);
}