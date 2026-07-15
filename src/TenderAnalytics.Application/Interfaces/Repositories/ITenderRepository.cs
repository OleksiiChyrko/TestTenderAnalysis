using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Application.Interfaces.Repositories;

public interface ITenderRepository
{
    Task UpsertAsync(
        Tender tender,
        CancellationToken cancellationToken = default);
}