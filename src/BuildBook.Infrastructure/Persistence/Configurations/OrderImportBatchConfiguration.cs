using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderImportBatchConfiguration : IEntityTypeConfiguration<OrderImportBatch>
{
    public void Configure(EntityTypeBuilder<OrderImportBatch> builder)
    {
        builder.ToTable("OrderImportBatches");

        builder.HasKey(orderImportBatch => orderImportBatch.Id);

        builder.Property(orderImportBatch => orderImportBatch.FileName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(orderImportBatch => orderImportBatch.PlanId)
            .HasMaxLength(128);

        builder.Property(orderImportBatch => orderImportBatch.PlanName)
            .HasMaxLength(256);

        builder.HasIndex(orderImportBatch => orderImportBatch.PlanId);
        builder.HasIndex(orderImportBatch => orderImportBatch.ImportedAt);

        builder.HasOne(orderImportBatch => orderImportBatch.ImportedByUser)
            .WithMany()
            .HasForeignKey(orderImportBatch => orderImportBatch.ImportedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(orderImportBatch => orderImportBatch.WarningEntries)
            .WithOne(orderImportWarning => orderImportWarning.OrderImportBatch)
            .HasForeignKey(orderImportWarning => orderImportWarning.OrderImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
