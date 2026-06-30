using BuildBook.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(customer => customer.AccountCode)
            .HasMaxLength(64);

        builder.Property(customer => customer.AddressLine1)
            .HasMaxLength(256);

        builder.Property(customer => customer.AddressLine2)
            .HasMaxLength(256);

        builder.Property(customer => customer.TownCity)
            .HasMaxLength(128);

        builder.Property(customer => customer.CountyRegion)
            .HasMaxLength(128);

        builder.Property(customer => customer.Postcode)
            .HasMaxLength(32);

        builder.Property(customer => customer.Country)
            .HasMaxLength(128);

        builder.Property(customer => customer.MainPhone)
            .HasMaxLength(64);

        builder.Property(customer => customer.MainEmail)
            .HasMaxLength(256);

        builder.Property(customer => customer.Website)
            .HasMaxLength(256);

        builder.Property(customer => customer.PrimaryContactName)
            .HasMaxLength(256);

        builder.Property(customer => customer.PrimaryContactEmail)
            .HasMaxLength(256);

        builder.Property(customer => customer.PrimaryContactPhone)
            .HasMaxLength(64);

        builder.Property(customer => customer.SupportContractStatus)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(customer => customer.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(customer => customer.LastUpdatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(customer => customer.Name);
        builder.HasIndex(customer => customer.SupportContractLevelId);
        builder.HasIndex(customer => customer.SupportContractStatus);
        builder.HasIndex(customer => customer.IsActive);

        builder.HasMany(customer => customer.BuildRecords)
            .WithOne(buildRecord => buildRecord.Customer)
            .HasForeignKey(buildRecord => buildRecord.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(customer => customer.RmaRecords)
            .WithOne(rmaRecord => rmaRecord.Customer)
            .HasForeignKey(rmaRecord => rmaRecord.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
