using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaStatusHistoryConfiguration : IEntityTypeConfiguration<RmaStatusHistory>
{
    public void Configure(EntityTypeBuilder<RmaStatusHistory> builder)
    {
        builder.ToTable("RmaStatusHistory");

        builder.HasKey(statusHistory => statusHistory.Id);

        builder.Property(statusHistory => statusHistory.ChangedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(statusHistory => statusHistory.Reason)
            .HasMaxLength(1024);

        builder.HasIndex(statusHistory => statusHistory.RmaRecordId);
        builder.HasIndex(statusHistory => statusHistory.ChangedAt);
    }
}
