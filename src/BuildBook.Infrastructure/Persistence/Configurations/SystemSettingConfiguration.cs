using BuildBook.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Key)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(setting => setting.Value)
            .HasMaxLength(2048);

        builder.Property(setting => setting.Description)
            .HasMaxLength(512);

        builder.Property(setting => setting.LastUpdatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(setting => setting.Key)
            .IsUnique();
    }
}
