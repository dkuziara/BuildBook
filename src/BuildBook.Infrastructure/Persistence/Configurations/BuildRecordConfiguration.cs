using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class BuildRecordConfiguration : IEntityTypeConfiguration<BuildRecord>
{
    public void Configure(EntityTypeBuilder<BuildRecord> builder)
    {
        builder.HasIndex(buildRecord => buildRecord.SerialNumber);
        builder.HasIndex(buildRecord => buildRecord.ProductCode);
        builder.HasIndex(buildRecord => buildRecord.ProductName);
        builder.HasIndex(buildRecord => buildRecord.CustomerId);
        builder.HasIndex(buildRecord => buildRecord.MachineName);
        builder.HasIndex(buildRecord => buildRecord.InvoiceNumber);
        builder.HasIndex(buildRecord => buildRecord.CustomerOrder);
        builder.HasIndex(buildRecord => buildRecord.OANumber);
        builder.HasIndex(buildRecord => buildRecord.RadSightVersion);
        builder.HasIndex(buildRecord => buildRecord.WindowsVersion);
        builder.HasIndex(buildRecord => buildRecord.DateShipped);
        builder.HasIndex(buildRecord => buildRecord.LastUpdatedAt);
    }
}
