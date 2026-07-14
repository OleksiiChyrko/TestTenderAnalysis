using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Infrastructure.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasMaxLength(32)
            .ValueGeneratedNever();

        builder.Property(x => x.TenderId)
            .HasColumnName("tender_id")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.AwardId)
            .HasColumnName("award_id")
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(32);

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(19, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.HasIndex(x => x.TenderId)
            .HasDatabaseName("ix_contracts_tender_id");

        builder.HasIndex(x => x.AwardId)
            .HasDatabaseName("ix_contracts_award_id");
    }
}