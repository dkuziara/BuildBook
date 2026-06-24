using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class ProductDetailsUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IProductDetailsUpdater
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

        var auditEntries = CreateAuditEntries(
            buildRecord,
            productCode,
            productName,
            productClassification,
            serialNumber,
            request.InternalStatus,
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

    private static List<BuildRecordAudit> CreateAuditEntries(
        BuildRecord buildRecord,
        string productCode,
        string productName,
        string? productClassification,
        string serialNumber,
        InternalStatus? internalStatus,
        string userName)
    {
        var auditEntries = new List<BuildRecordAudit>();

        AddAuditEntryIfChanged(auditEntries, buildRecord, "ProductCode", buildRecord.ProductCode, productCode, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "ProductName", buildRecord.ProductName, productName, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "ProductClassification", buildRecord.ProductClassification, productClassification, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "SerialNumber", buildRecord.SerialNumber, serialNumber, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "InternalStatus", buildRecord.InternalStatus?.ToString(), internalStatus?.ToString(), userName);

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
