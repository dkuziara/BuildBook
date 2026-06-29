using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaAuditConfiguration : IEntityTypeConfiguration<RmaAudit>
{
    public void Configure(EntityTypeBuilder<RmaAudit> builder)
    {
        builder.ToTable("RmaAudit");

        builder.HasKey(auditEntry => auditEntry.Id);

        builder.Property(auditEntry => auditEntry.User)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(auditEntry => auditEntry.Action)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(auditEntry => auditEntry.FieldChanged)
            .HasMaxLength(128);

        builder.Property(auditEntry => auditEntry.OldValue)
            .HasMaxLength(2000);

        builder.Property(auditEntry => auditEntry.NewValue)
            .HasMaxLength(2000);

        builder.Property(auditEntry => auditEntry.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(auditEntry => auditEntry.RmaRecordId);
        builder.HasIndex(auditEntry => auditEntry.OccurredAt);
    }
}
