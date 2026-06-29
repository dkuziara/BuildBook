using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaNoteConfiguration : IEntityTypeConfiguration<RmaNote>
{
    public void Configure(EntityTypeBuilder<RmaNote> builder)
    {
        builder.ToTable("RmaNotes");

        builder.HasKey(note => note.Id);

        builder.Property(note => note.NoteText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(note => note.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(note => note.LastUpdatedBy)
            .HasMaxLength(256);

        builder.HasIndex(note => note.RmaRecordId);
        builder.HasIndex(note => note.NoteType);
    }
}
