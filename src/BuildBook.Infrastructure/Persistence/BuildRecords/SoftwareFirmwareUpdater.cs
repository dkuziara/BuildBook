using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class SoftwareFirmwareUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : ISoftwareFirmwareUpdater
{
    public async Task<UpdateSoftwareFirmwareResult> UpdateAsync(
        int buildRecordId,
        UpdateSoftwareFirmwareRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = string.IsNullOrWhiteSpace(updatedBy) ? "Unknown" : updatedBy.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .SingleOrDefaultAsync(
                record => record.Id == buildRecordId && record.IsActive,
                cancellationToken);

        if (buildRecord is null)
        {
            return UpdateSoftwareFirmwareResult.Failure("Build Record was not found.");
        }

        var diskImageVersion = NormalizeOptionalValue(request.DiskImageVersion);
        var radSightVersion = NormalizeOptionalValue(request.RadSightVersion);
        var windowsVersion = NormalizeOptionalValue(request.WindowsVersion);
        var windowsLatestPatch = NormalizeOptionalValue(request.WindowsLatestPatch);
        var bleuvioFirmwareVersion = NormalizeOptionalValue(request.BleuvioFirmwareVersion);
        var charthouseIrdaFirmwareVersion = NormalizeOptionalValue(request.CharthouseIrdaFirmwareVersion);
        var radioFirmware = NormalizeOptionalValue(request.RadioFirmware);

        var auditEntries = CreateAuditEntries(
            buildRecord,
            diskImageVersion,
            radSightVersion,
            windowsVersion,
            windowsLatestPatch,
            bleuvioFirmwareVersion,
            charthouseIrdaFirmwareVersion,
            radioFirmware,
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateSoftwareFirmwareResult.Success();
        }

        buildRecord.DiskImageVersion = diskImageVersion;
        buildRecord.RadSightVersion = radSightVersion;
        buildRecord.WindowsVersion = windowsVersion;
        buildRecord.WindowsLatestPatch = windowsLatestPatch;
        buildRecord.BleuvioFirmwareVersion = bleuvioFirmwareVersion;
        buildRecord.CharthouseIrdaFirmwareVersion = charthouseIrdaFirmwareVersion;
        buildRecord.RadioFirmware = radioFirmware;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateSoftwareFirmwareResult.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<BuildRecordAudit> CreateAuditEntries(
        BuildRecord buildRecord,
        string? diskImageVersion,
        string? radSightVersion,
        string? windowsVersion,
        string? windowsLatestPatch,
        string? bleuvioFirmwareVersion,
        string? charthouseIrdaFirmwareVersion,
        string? radioFirmware,
        string userName)
    {
        var auditEntries = new List<BuildRecordAudit>();

        AddAuditEntryIfChanged(auditEntries, buildRecord, "DiskImageVersion", buildRecord.DiskImageVersion, diskImageVersion, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "RadSightVersion", buildRecord.RadSightVersion, radSightVersion, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "WindowsVersion", buildRecord.WindowsVersion, windowsVersion, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "WindowsLatestPatch", buildRecord.WindowsLatestPatch, windowsLatestPatch, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "BleuvioFirmwareVersion", buildRecord.BleuvioFirmwareVersion, bleuvioFirmwareVersion, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "CharthouseIrdaFirmwareVersion", buildRecord.CharthouseIrdaFirmwareVersion, charthouseIrdaFirmwareVersion, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "RadioFirmware", buildRecord.RadioFirmware, radioFirmware, userName);

        return auditEntries;
    }

    private static void AddAuditEntryIfChanged(
        ICollection<BuildRecordAudit> auditEntries,
        BuildRecord buildRecord,
        string fieldChanged,
        string? oldValue,
        string? newValue,
        string userName)
    {
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        auditEntries.Add(new BuildRecordAudit
        {
            BuildRecordId = buildRecord.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            User = userName,
            Action = AuditAction.Updated,
            FieldChanged = fieldChanged,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
}
