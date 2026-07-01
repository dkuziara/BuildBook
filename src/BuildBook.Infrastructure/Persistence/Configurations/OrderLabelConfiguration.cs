using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderLabelConfiguration : IEntityTypeConfiguration<OrderLabel>
{
    public void Configure(EntityTypeBuilder<OrderLabel> builder)
    {
        builder.ToTable("OrderLabels");

        builder.HasKey(orderLabel => orderLabel.Id);

        builder.Property(orderLabel => orderLabel.LabelText)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(orderLabel => orderLabel.Source)
            .HasMaxLength(128);

        builder.HasIndex(orderLabel => orderLabel.OrderRecordId);
        builder.HasIndex(orderLabel => orderLabel.LabelText);
    }
}
