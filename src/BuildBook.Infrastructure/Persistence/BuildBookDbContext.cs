using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Domain.Security;
using BuildBook.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence;

public sealed class BuildBookDbContext(DbContextOptions<BuildBookDbContext> options) : DbContext(options)
{
    public DbSet<BuildRecord> BuildRecords => Set<BuildRecord>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<SupportContractLevel> SupportContractLevels => Set<SupportContractLevel>();

    public DbSet<BuildRecordSecret> BuildRecordSecrets => Set<BuildRecordSecret>();

    public DbSet<BuildRecordAudit> BuildRecordAudit => Set<BuildRecordAudit>();

    public DbSet<ImportBatch> Imports => Set<ImportBatch>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();

    public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<RmaRecord> RmaRecords => Set<RmaRecord>();

    public DbSet<RmaChecklistItem> RmaChecklistItems => Set<RmaChecklistItem>();

    public DbSet<RmaNote> RmaNotes => Set<RmaNote>();

    public DbSet<RmaCommunication> RmaCommunications => Set<RmaCommunication>();

    public DbSet<RmaAttachment> RmaAttachments => Set<RmaAttachment>();

    public DbSet<RmaPart> RmaParts => Set<RmaPart>();

    public DbSet<RmaStatusHistory> RmaStatusHistory => Set<RmaStatusHistory>();

    public DbSet<RmaAudit> RmaAudit => Set<RmaAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuildBookDbContext).Assembly);
    }
}
