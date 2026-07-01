using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderAssignmentConfiguration : IEntityTypeConfiguration<OrderAssignment>
{
    public void Configure(EntityTypeBuilder<OrderAssignment> builder)
    {
        builder.ToTable("OrderAssignments");

        builder.HasKey(orderAssignment => orderAssignment.Id);

        builder.Property(orderAssignment => orderAssignment.ImportedUserText)
            .HasMaxLength(256);

        builder.HasIndex(orderAssignment => orderAssignment.OrderRecordId);
        builder.HasIndex(orderAssignment => orderAssignment.ApplicationUserId);
        builder.HasIndex(orderAssignment => orderAssignment.AssignedAt);

        builder.HasOne(orderAssignment => orderAssignment.ApplicationUser)
            .WithMany()
            .HasForeignKey(orderAssignment => orderAssignment.ApplicationUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderAssignment => orderAssignment.AssignedByUser)
            .WithMany()
            .HasForeignKey(orderAssignment => orderAssignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
