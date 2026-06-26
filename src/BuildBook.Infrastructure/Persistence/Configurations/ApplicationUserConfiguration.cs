using BuildBook.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.WindowsUserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(256);

        builder.Property(user => user.EmailAddress)
            .HasMaxLength(256);

        builder.Property(user => user.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.LastUpdatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.WindowsUserName)
            .IsUnique();

        builder.HasMany(user => user.UserRoles)
            .WithOne(userRole => userRole.ApplicationUser)
            .HasForeignKey(userRole => userRole.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
