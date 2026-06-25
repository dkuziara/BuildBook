using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class NetworkNotesUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : INetworkNotesUpdater
{
    public async Task<UpdateNetworkNotesResult> UpdateAsync(
        int buildRecordId,
        UpdateNetworkNotesRequest request,
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
            return UpdateNetworkNotesResult.Failure("Build Record was not found.");
        }

        var wifiSsid = NormalizeOptionalValue(request.WifiSsid);
        var routerUsed = NormalizeOptionalValue(request.RouterUsed);
        var note = NormalizeOptionalValue(request.Note);

        var auditEntries = buildRecordAuditService.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("WifiSsid", buildRecord.WifiSsid, wifiSsid),
                new BuildRecordAuditChange("RouterUsed", buildRecord.RouterUsed, routerUsed),
                new BuildRecordAuditChange("Note", buildRecord.Note, note)
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateNetworkNotesResult.Success();
        }

        buildRecord.WifiSsid = wifiSsid;
        buildRecord.RouterUsed = routerUsed;
        buildRecord.Note = note;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateNetworkNotesResult.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
