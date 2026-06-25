using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class ProductDetailsUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : IProductDetailsUpdater
{
    public async Task<UpdateProductDetailsResult> UpdateAsync(
        int buildRecordId,
        UpdateProductDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = UpdateProductDetailsValidator.Validate(request);

        if (validationErrors.Count > 0)
        {
            return UpdateProductDetailsResult.Failure([.. validationErrors]);
        }

        var productCode = request.ProductCode.Trim();
        var productName = request.ProductName.Trim();
        var productClassification = NormalizeOptionalValue(request.ProductClassification);
        var serialNumber = request.SerialNumber.Trim();
        var userName = string.IsNullOrWhiteSpace(updatedBy) ? "Unknown" : updatedBy.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .SingleOrDefaultAsync(
                record => record.Id == buildRecordId && record.IsActive,
                cancellationToken);

        if (buildRecord is null)
        {
            return UpdateProductDetailsResult.Failure("Build Record was not found.");
        }

        if (await dbContext.BuildRecords.AnyAsync(
            record => record.Id != buildRecordId
                && record.IsActive
                && record.SerialNumber == serialNumber,
            cancellationToken))
        {
            return UpdateProductDetailsResult.Failure("A Build Record with this serial number already exists.");
        }

        var auditEntries = buildRecordAuditService.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("ProductCode", buildRecord.ProductCode, productCode),
                new BuildRecordAuditChange("ProductName", buildRecord.ProductName, productName),
                new BuildRecordAuditChange("ProductClassification", buildRecord.ProductClassification, productClassification),
                new BuildRecordAuditChange("SerialNumber", buildRecord.SerialNumber, serialNumber),
                new BuildRecordAuditChange("InternalStatus", buildRecord.InternalStatus?.ToString(), request.InternalStatus?.ToString())
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateProductDetailsResult.Success();
        }

        buildRecord.ProductCode = productCode;
        buildRecord.ProductName = productName;
        buildRecord.ProductClassification = productClassification;
        buildRecord.SerialNumber = serialNumber;
        buildRecord.InternalStatus = request.InternalStatus;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateProductDetailsResult.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
