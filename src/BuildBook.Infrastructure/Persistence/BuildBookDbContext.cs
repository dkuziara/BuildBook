using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence;

public sealed class BuildBookDbContext(DbContextOptions<BuildBookDbContext> options) : DbContext(options)
{
    public DbSet<BuildRecord> BuildRecords => Set<BuildRecord>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<BuildRecordSecret> BuildRecordSecrets => Set<BuildRecordSecret>();

    public DbSet<BuildRecordAudit> BuildRecordAudit => Set<BuildRecordAudit>();

    public DbSet<ImportBatch> Imports => Set<ImportBatch>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();

    public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuildBookDbContext).Assembly);
    }
}
