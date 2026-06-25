using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class HardwareDetailsUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : IHardwareDetailsUpdater
{
    public async Task<UpdateHardwareDetailsResult> UpdateAsync(
        int buildRecordId,
        UpdateHardwareDetailsRequest request,
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
            return UpdateHardwareDetailsResult.Failure("Build Record was not found.");
        }

        var panelDeviceModel = NormalizeOptionalValue(request.PanelDeviceModel);
        var panelDeviceSerial = NormalizeOptionalValue(request.PanelDeviceSerial);
        var panelFirmwareVersion = NormalizeOptionalValue(request.PanelFirmwareVersion);
        var machineName = NormalizeOptionalValue(request.MachineName);
        var radioSerialNumber = NormalizeOptionalValue(request.RadioSerialNumber);
        var hardwareNotes = NormalizeOptionalValue(request.HardwareNotes);

        var warnings = new List<string>();

        if (machineName is not null
            && await dbContext.BuildRecords.AnyAsync(
                record => record.Id != buildRecordId
                    && record.IsActive
                    && record.MachineName == machineName,
                cancellationToken))
        {
            warnings.Add("Another Build Record already uses this machine name.");
        }

        var auditEntries = buildRecordAuditService.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("PanelDeviceModel", buildRecord.PanelDeviceModel, panelDeviceModel),
                new BuildRecordAuditChange("PanelDeviceSerial", buildRecord.PanelDeviceSerial, panelDeviceSerial),
                new BuildRecordAuditChange("PanelFirmwareVersion", buildRecord.PanelFirmwareVersion, panelFirmwareVersion),
                new BuildRecordAuditChange("MachineName", buildRecord.MachineName, machineName),
                new BuildRecordAuditChange("RadioSerialNumber", buildRecord.RadioSerialNumber, radioSerialNumber),
                new BuildRecordAuditChange("HardwareNotes", buildRecord.HardwareNotes, hardwareNotes)
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateHardwareDetailsResult.Success(warnings.ToArray());
        }

        buildRecord.PanelDeviceModel = panelDeviceModel;
        buildRecord.PanelDeviceSerial = panelDeviceSerial;
        buildRecord.PanelFirmwareVersion = panelFirmwareVersion;
        buildRecord.MachineName = machineName;
        buildRecord.RadioSerialNumber = radioSerialNumber;
        buildRecord.HardwareNotes = hardwareNotes;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateHardwareDetailsResult.Success(warnings.ToArray());
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
