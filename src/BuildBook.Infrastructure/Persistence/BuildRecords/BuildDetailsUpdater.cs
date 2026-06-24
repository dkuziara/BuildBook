using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildDetailsUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildDetailsUpdater
{
    public async Task<UpdateBuildDetailsResult> UpdateAsync(
        int buildRecordId,
        UpdateBuildDetailsRequest request,
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
            return UpdateBuildDetailsResult.Failure("Build Record was not found.");
        }

        var assembledIn = NormalizeOptionalValue(request.AssembledIn);
        var assembledBy = NormalizeOptionalValue(request.AssembledBy);
        var hardwareManufacturer = NormalizeOptionalValue(request.HardwareManufacturer);
        var manufacturerPartNumber = NormalizeOptionalValue(request.ManufacturerPartNumber);
        var manufacturerRevision = NormalizeOptionalValue(request.ManufacturerRevision);
        var manufacturerSerialNumber = NormalizeOptionalValue(request.ManufacturerSerialNumber);
        var packingList = NormalizeOptionalValue(request.PackingList);
        var checkedBy = NormalizeOptionalValue(request.CheckedBy);

        var auditEntries = CreateAuditEntries(
            buildRecord,
            assembledIn,
            assembledBy,
            request.DateAssembled,
            hardwareManufacturer,
            manufacturerPartNumber,
            manufacturerRevision,
            manufacturerSerialNumber,
            packingList,
            checkedBy,
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateBuildDetailsResult.Success();
        }

        buildRecord.AssembledIn = assembledIn;
        buildRecord.AssembledBy = assembledBy;
        buildRecord.DateAssembled = request.DateAssembled;
        buildRecord.HardwareManufacturer = hardwareManufacturer;
        buildRecord.ManufacturerPartNumber = manufacturerPartNumber;
        buildRecord.ManufacturerRevision = manufacturerRevision;
        buildRecord.ManufacturerSerialNumber = manufacturerSerialNumber;
        buildRecord.PackingList = packingList;
        buildRecord.CheckedBy = checkedBy;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateBuildDetailsResult.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<BuildRecordAudit> CreateAuditEntries(
        BuildRecord buildRecord,
        string? assembledIn,
        string? assembledBy,
        DateOnly? dateAssembled,
        string? hardwareManufacturer,
        string? manufacturerPartNumber,
        string? manufacturerRevision,
        string? manufacturerSerialNumber,
        string? packingList,
        string? checkedBy,
        string userName)
    {
        var auditEntries = new List<BuildRecordAudit>();

        AddAuditEntryIfChanged(auditEntries, buildRecord, "AssembledIn", buildRecord.AssembledIn, assembledIn, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "AssembledBy", buildRecord.AssembledBy, assembledBy, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "DateAssembled", FormatDate(buildRecord.DateAssembled), FormatDate(dateAssembled), userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "HardwareManufacturer", buildRecord.HardwareManufacturer, hardwareManufacturer, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "ManufacturerPartNumber", buildRecord.ManufacturerPartNumber, manufacturerPartNumber, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "ManufacturerRevision", buildRecord.ManufacturerRevision, manufacturerRevision, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "ManufacturerSerialNumber", buildRecord.ManufacturerSerialNumber, manufacturerSerialNumber, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "PackingList", buildRecord.PackingList, packingList, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "CheckedBy", buildRecord.CheckedBy, checkedBy, userName);

        return auditEntries;
    }

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
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
