using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordCreator(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : IBuildRecordCreator
{
    public async Task<CreateBuildRecordResult> CreateAsync(
        CreateBuildRecordRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = CreateBuildRecordValidator.Validate(request);

        if (validationErrors.Count > 0)
        {
            return CreateBuildRecordResult.Failure([.. validationErrors]);
        }

        var productCode = request.ProductCode.Trim();
        var productName = request.ProductName.Trim();
        var serialNumber = request.SerialNumber.Trim();
        var userName = string.IsNullOrWhiteSpace(createdBy) ? "Unknown" : createdBy.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (await dbContext.BuildRecords.AnyAsync(
            buildRecord => buildRecord.SerialNumber == serialNumber,
            cancellationToken))
        {
            return CreateBuildRecordResult.Failure("A Build Record with this serial number already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var buildRecord = new BuildRecord
        {
            ProductCode = productCode,
            ProductName = productName,
            SerialNumber = serialNumber,
            CreatedAt = now,
            CreatedBy = userName,
            LastUpdatedAt = now,
            LastUpdatedBy = userName,
            IsActive = true
        };

        dbContext.BuildRecords.Add(buildRecord);
        dbContext.BuildRecordAudit.Add(buildRecordAuditService.CreateRecordCreatedEntry(buildRecord, userName));
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreateBuildRecordResult.Success(buildRecord.Id);
    }
}
