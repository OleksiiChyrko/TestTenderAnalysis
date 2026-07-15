using TenderAnalytics.Application.DTOs.Import;

namespace TenderAnalytics.Application.Interfaces.Services;

public interface ITenderImportService
{
    Task<bool> ImportTenderAsync(
        string tenderId,
        CancellationToken cancellationToken = default);

    Task<ImportResult> ImportFeedAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default);
}