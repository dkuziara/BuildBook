using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaAttachmentConfiguration : IEntityTypeConfiguration<RmaAttachment>
{
    public void Configure(EntityTypeBuilder<RmaAttachment> builder)
    {
        builder.ToTable("RmaAttachments");

        builder.HasKey(attachment => attachment.Id);

        builder.Property(attachment => attachment.FileName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(attachment => attachment.StoredFilePath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(attachment => attachment.ContentType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(attachment => attachment.AttachmentType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(attachment => attachment.Description)
            .HasMaxLength(1024);

        builder.Property(attachment => attachment.UploadedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(attachment => attachment.RmaRecordId);
        builder.HasIndex(attachment => attachment.UploadedAt);
    }
}
