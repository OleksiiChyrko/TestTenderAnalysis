using Microsoft.EntityFrameworkCore;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tender> Tenders => Set<Tender>();

    public DbSet<Contract> Contracts => Set<Contract>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<ContractSupplier> ContractSuppliers =>
        Set<ContractSupplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}