using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaChecklistItemConfiguration : IEntityTypeConfiguration<RmaChecklistItem>
{
    public void Configure(EntityTypeBuilder<RmaChecklistItem> builder)
    {
        builder.ToTable("RmaChecklistItems");

        builder.HasKey(checklistItem => checklistItem.Id);

        builder.Property(checklistItem => checklistItem.Text)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(checklistItem => checklistItem.CompletedBy)
            .HasMaxLength(256);

        builder.HasIndex(checklistItem => new { checklistItem.RmaRecordId, checklistItem.DisplayOrder })
            .IsUnique();
    }
}
