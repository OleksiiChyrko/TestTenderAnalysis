using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Identifier)
            .HasColumnName("identifier")
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => x.Identifier)
            .IsUnique()
            .HasFilter("\"identifier\" IS NOT NULL")
            .HasDatabaseName("ux_suppliers_identifier");

        builder.HasIndex(x => x.NormalizedName)
            .HasDatabaseName("ix_suppliers_normalized_name");
    }
}