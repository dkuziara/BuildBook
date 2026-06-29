using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaCommunicationConfiguration : IEntityTypeConfiguration<RmaCommunication>
{
    public void Configure(EntityTypeBuilder<RmaCommunication> builder)
    {
        builder.ToTable("RmaCommunications");

        builder.HasKey(communication => communication.Id);

        builder.Property(communication => communication.ContactMethod)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(communication => communication.ContactPerson)
            .HasMaxLength(256);

        builder.Property(communication => communication.Summary)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(communication => communication.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(communication => communication.RmaRecordId);
        builder.HasIndex(communication => communication.CommunicationDate);
    }
}
