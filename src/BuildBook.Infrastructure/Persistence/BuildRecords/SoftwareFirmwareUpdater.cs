using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class SoftwareFirmwareUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : ISoftwareFirmwareUpdater
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

        var auditEntries = buildRecordAuditService.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("DiskImageVersion", buildRecord.DiskImageVersion, diskImageVersion),
                new BuildRecordAuditChange("RadSightVersion", buildRecord.RadSightVersion, radSightVersion),
                new BuildRecordAuditChange("WindowsVersion", buildRecord.WindowsVersion, windowsVersion),
                new BuildRecordAuditChange("WindowsLatestPatch", buildRecord.WindowsLatestPatch, windowsLatestPatch),
                new BuildRecordAuditChange("BleuvioFirmwareVersion", buildRecord.BleuvioFirmwareVersion, bleuvioFirmwareVersion),
                new BuildRecordAuditChange("CharthouseIrdaFirmwareVersion", buildRecord.CharthouseIrdaFirmwareVersion, charthouseIrdaFirmwareVersion),
                new BuildRecordAuditChange("RadioFirmware", buildRecord.RadioFirmware, radioFirmware)
            ],
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
}
