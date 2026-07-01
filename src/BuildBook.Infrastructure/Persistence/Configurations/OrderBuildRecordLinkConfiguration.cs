using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderBuildRecordLinkConfiguration : IEntityTypeConfiguration<OrderBuildRecordLink>
{
    public void Configure(EntityTypeBuilder<OrderBuildRecordLink> builder)
    {
        builder.ToTable("OrderBuildRecordLinks");

        builder.HasKey(orderBuildRecordLink => orderBuildRecordLink.Id);

        builder.Property(orderBuildRecordLink => orderBuildRecordLink.LinkType)
            .HasMaxLength(64);

        builder.HasIndex(orderBuildRecordLink => orderBuildRecordLink.OrderRecordId);
        builder.HasIndex(orderBuildRecordLink => orderBuildRecordLink.BuildRecordId);
        builder.HasIndex(orderBuildRecordLink => new { orderBuildRecordLink.OrderRecordId, orderBuildRecordLink.BuildRecordId })
            .IsUnique();

        builder.HasOne(orderBuildRecordLink => orderBuildRecordLink.BuildRecord)
            .WithMany(buildRecord => buildRecord.OrderLinks)
            .HasForeignKey(orderBuildRecordLink => orderBuildRecordLink.BuildRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(orderBuildRecordLink => orderBuildRecordLink.LinkedByUser)
            .WithMany()
            .HasForeignKey(orderBuildRecordLink => orderBuildRecordLink.LinkedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
