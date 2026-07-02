using BuildBook.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.ProductCode)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(product => product.Description)
            .HasMaxLength(256);

        builder.Property(product => product.Notes)
            .HasMaxLength(4000);

        builder.Property(product => product.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(product => product.LastUpdatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(product => product.ProductCode)
            .IsUnique();

        builder.HasIndex(product => product.LastUpdatedAt);
    }
}
