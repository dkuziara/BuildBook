using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderChecklistItemConfiguration : IEntityTypeConfiguration<OrderChecklistItem>
{
    public void Configure(EntityTypeBuilder<OrderChecklistItem> builder)
    {
        builder.ToTable("OrderChecklistItems");

        builder.HasKey(orderChecklistItem => orderChecklistItem.Id);

        builder.Property(orderChecklistItem => orderChecklistItem.Text)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(orderChecklistItem => orderChecklistItem.Source)
            .HasMaxLength(128);

        builder.Property(orderChecklistItem => orderChecklistItem.ImportedCompletedText)
            .HasMaxLength(256);

        builder.HasIndex(orderChecklistItem => orderChecklistItem.OrderRecordId);
        builder.HasIndex(orderChecklistItem => orderChecklistItem.DisplayOrder);
        builder.HasIndex(orderChecklistItem => orderChecklistItem.IsCompleted);
        builder.HasIndex(orderChecklistItem => orderChecklistItem.ShowInBoardView);

        builder.HasOne(orderChecklistItem => orderChecklistItem.CompletedByUser)
            .WithMany()
            .HasForeignKey(orderChecklistItem => orderChecklistItem.CompletedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
