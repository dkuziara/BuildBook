using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class BuildRecordSecretConfiguration : IEntityTypeConfiguration<BuildRecordSecret>
{
    public void Configure(EntityTypeBuilder<BuildRecordSecret> builder)
    {
        builder.HasIndex(secret => new { secret.BuildRecordId, secret.SecretType })
            .IsUnique();
    }
}
