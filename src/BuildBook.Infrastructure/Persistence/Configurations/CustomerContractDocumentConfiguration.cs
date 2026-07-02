using BuildBook.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class CustomerContractDocumentConfiguration : IEntityTypeConfiguration<CustomerContractDocument>
{
    public void Configure(EntityTypeBuilder<CustomerContractDocument> builder)
    {
        builder.ToTable("CustomerContractDocuments");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.FileName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(document => document.StoredFilePath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(document => document.ContentType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(document => document.DocumentType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(document => document.Description)
            .HasMaxLength(1024);

        builder.Property(document => document.UploadedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(document => document.CustomerId);
        builder.HasIndex(document => document.UploadedAt);
    }
}
