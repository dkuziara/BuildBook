using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class HardwareDetailsUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IHardwareDetailsUpdater
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
        var routerUsed = NormalizeOptionalValue(request.RouterUsed);
        var hardwareNotes = NormalizeOptionalValue(request.HardwareNotes);

        if (machineName is not null
            && await dbContext.BuildRecords.AnyAsync(
                record => record.Id != buildRecordId
                    && record.IsActive
                    && record.MachineName == machineName,
                cancellationToken))
        {
            return UpdateHardwareDetailsResult.Failure("A Build Record with this machine name already exists.");
        }

        var auditEntries = CreateAuditEntries(
            buildRecord,
            panelDeviceModel,
            panelDeviceSerial,
            panelFirmwareVersion,
            machineName,
            radioSerialNumber,
            routerUsed,
            hardwareNotes,
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateHardwareDetailsResult.Success();
        }

        buildRecord.PanelDeviceModel = panelDeviceModel;
        buildRecord.PanelDeviceSerial = panelDeviceSerial;
        buildRecord.PanelFirmwareVersion = panelFirmwareVersion;
        buildRecord.MachineName = machineName;
        buildRecord.RadioSerialNumber = radioSerialNumber;
        buildRecord.RouterUsed = routerUsed;
        buildRecord.HardwareNotes = hardwareNotes;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateHardwareDetailsResult.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<BuildRecordAudit> CreateAuditEntries(
        BuildRecord buildRecord,
        string? panelDeviceModel,
        string? panelDeviceSerial,
        string? panelFirmwareVersion,
        string? machineName,
        string? radioSerialNumber,
        string? routerUsed,
        string? hardwareNotes,
        string userName)
    {
        var auditEntries = new List<BuildRecordAudit>();

        AddAuditEntryIfChanged(auditEntries, buildRecord, "PanelDeviceModel", buildRecord.PanelDeviceModel, panelDeviceModel, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "PanelDeviceSerial", buildRecord.PanelDeviceSerial, panelDeviceSerial, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "PanelFirmwareVersion", buildRecord.PanelFirmwareVersion, panelFirmwareVersion, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "MachineName", buildRecord.MachineName, machineName, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "RadioSerialNumber", buildRecord.RadioSerialNumber, radioSerialNumber, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "RouterUsed", buildRecord.RouterUsed, routerUsed, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "HardwareNotes", buildRecord.HardwareNotes, hardwareNotes, userName);

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
