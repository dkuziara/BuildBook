using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderImportWarningConfiguration : IEntityTypeConfiguration<OrderImportWarning>
{
    public void Configure(EntityTypeBuilder<OrderImportWarning> builder)
    {
        builder.ToTable("OrderImportWarnings");

        builder.HasKey(orderImportWarning => orderImportWarning.Id);

        builder.Property(orderImportWarning => orderImportWarning.PlannerTaskId)
            .HasMaxLength(128);

        builder.Property(orderImportWarning => orderImportWarning.WarningType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(orderImportWarning => orderImportWarning.Message)
            .HasMaxLength(2048)
            .IsRequired();

        builder.HasIndex(orderImportWarning => orderImportWarning.OrderImportBatchId);
        builder.HasIndex(orderImportWarning => orderImportWarning.PlannerTaskId);
        builder.HasIndex(orderImportWarning => orderImportWarning.Severity);
    }
}
