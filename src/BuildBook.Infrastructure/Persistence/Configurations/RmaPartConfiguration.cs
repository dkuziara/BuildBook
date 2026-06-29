using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaPartConfiguration : IEntityTypeConfiguration<RmaPart>
{
    public void Configure(EntityTypeBuilder<RmaPart> builder)
    {
        builder.ToTable("RmaParts");

        builder.HasKey(part => part.Id);

        builder.Property(part => part.PartName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(part => part.PartNumber)
            .HasMaxLength(128);

        builder.Property(part => part.SerialNumber)
            .HasMaxLength(128);

        builder.Property(part => part.Supplier)
            .HasMaxLength(256);

        builder.Property(part => part.UnitCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(part => part.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(part => part.RmaRecordId);
    }
}
