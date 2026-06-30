using BuildBook.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class SupportContractLevelConfiguration : IEntityTypeConfiguration<SupportContractLevel>
{
    public void Configure(EntityTypeBuilder<SupportContractLevel> builder)
    {
        builder.ToTable("SupportContractLevels");

        builder.HasKey(level => level.Id);

        builder.Property(level => level.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(level => level.Description)
            .HasMaxLength(512);

        builder.Property(level => level.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(level => level.LastUpdatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(level => level.Name)
            .IsUnique();

        builder.HasIndex(level => level.DisplayOrder);
        builder.HasIndex(level => level.IsActive);

        builder.HasMany(level => level.Customers)
            .WithOne(customer => customer.SupportContractLevel)
            .HasForeignKey(customer => customer.SupportContractLevelId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
