using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Infrastructure.Persistence.Configurations;

public sealed class ContractSupplierConfiguration
    : IEntityTypeConfiguration<ContractSupplier>
{
    public void Configure(EntityTypeBuilder<ContractSupplier> builder)
    {
        builder.ToTable("contract_suppliers");

        builder.HasKey(x => new
        {
            x.ContractId,
            x.SupplierId
        });

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id")
            .HasMaxLength(32);

        builder.Property(x => x.SupplierId)
            .HasColumnName("supplier_id");

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.ContractSuppliers)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.ContractSuppliers)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SupplierId)
            .HasDatabaseName("ix_contract_suppliers_supplier_id");
    }
}