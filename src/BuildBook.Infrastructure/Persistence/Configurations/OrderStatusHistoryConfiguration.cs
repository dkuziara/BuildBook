using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");

        builder.HasKey(orderStatusHistory => orderStatusHistory.Id);

        builder.Property(orderStatusHistory => orderStatusHistory.OldStatus)
            .HasMaxLength(128);

        builder.Property(orderStatusHistory => orderStatusHistory.NewStatus)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(orderStatusHistory => orderStatusHistory.Reason)
            .HasMaxLength(1024);

        builder.HasIndex(orderStatusHistory => orderStatusHistory.OrderRecordId);
        builder.HasIndex(orderStatusHistory => orderStatusHistory.ChangedAt);

        builder.HasOne(orderStatusHistory => orderStatusHistory.ChangedByUser)
            .WithMany()
            .HasForeignKey(orderStatusHistory => orderStatusHistory.ChangedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
