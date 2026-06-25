using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildDetailsUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : IBuildDetailsUpdater
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

        var auditEntries = buildRecordAuditService.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("AssembledIn", buildRecord.AssembledIn, assembledIn),
                new BuildRecordAuditChange("AssembledBy", buildRecord.AssembledBy, assembledBy),
                new BuildRecordAuditChange("DateAssembled", FormatDate(buildRecord.DateAssembled), FormatDate(request.DateAssembled)),
                new BuildRecordAuditChange("HardwareManufacturer", buildRecord.HardwareManufacturer, hardwareManufacturer),
                new BuildRecordAuditChange("ManufacturerPartNumber", buildRecord.ManufacturerPartNumber, manufacturerPartNumber),
                new BuildRecordAuditChange("ManufacturerRevision", buildRecord.ManufacturerRevision, manufacturerRevision),
                new BuildRecordAuditChange("ManufacturerSerialNumber", buildRecord.ManufacturerSerialNumber, manufacturerSerialNumber),
                new BuildRecordAuditChange("PackingList", buildRecord.PackingList, packingList),
                new BuildRecordAuditChange("CheckedBy", buildRecord.CheckedBy, checkedBy)
            ],
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

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }
}
