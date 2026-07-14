using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Infrastructure.Persistence.Configurations;

public sealed class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> builder)
    {
        builder.ToTable("tenders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasMaxLength(32)
            .ValueGeneratedNever();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.DateCreated)
            .HasColumnName("date_created")
            .IsRequired();

        builder.Property(x => x.ExpectedAmount)
            .HasColumnName("expected_amount")
            .HasPrecision(19, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.CpvCode)
            .HasColumnName("cpv_code")
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.ProcuringEntityIdentifier)
            .HasColumnName("procuring_entity_identifier")
            .HasMaxLength(64);

        builder.Property(x => x.ProcuringEntityName)
            .HasColumnName("procuring_entity_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ImportedAt)
            .HasColumnName("imported_at")
            .IsRequired();

        builder.HasMany(x => x.Contracts)
            .WithOne(x => x.Tender)
            .HasForeignKey(x => x.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.CpvCode,
            x.Status,
            x.DateCreated
        })
        .HasDatabaseName("ix_tenders_cpv_status_date_created");

        builder.HasIndex(x => new
        {
            x.ProcuringEntityIdentifier,
            x.ProcuringEntityName
        })
        .HasDatabaseName("ix_tenders_procuring_entity");
    }
}