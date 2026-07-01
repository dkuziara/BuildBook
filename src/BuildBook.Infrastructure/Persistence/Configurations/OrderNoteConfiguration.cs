using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderNoteConfiguration : IEntityTypeConfiguration<OrderNote>
{
    public void Configure(EntityTypeBuilder<OrderNote> builder)
    {
        builder.ToTable("OrderNotes");

        builder.HasKey(orderNote => orderNote.Id);

        builder.Property(orderNote => orderNote.NoteText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(orderNote => orderNote.OrderRecordId);
        builder.HasIndex(orderNote => orderNote.NoteType);
        builder.HasIndex(orderNote => orderNote.CreatedAt);

        builder.HasOne(orderNote => orderNote.CreatedByUser)
            .WithMany()
            .HasForeignKey(orderNote => orderNote.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderNote => orderNote.LastUpdatedByUser)
            .WithMany()
            .HasForeignKey(orderNote => orderNote.LastUpdatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
