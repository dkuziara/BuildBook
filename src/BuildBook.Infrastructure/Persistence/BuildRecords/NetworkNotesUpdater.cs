using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class NetworkNotesUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : INetworkNotesUpdater
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

        var auditEntries = CreateAuditEntries(buildRecord, wifiSsid, routerUsed, note, userName);

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

    private static List<BuildRecordAudit> CreateAuditEntries(
        BuildRecord buildRecord,
        string? wifiSsid,
        string? routerUsed,
        string? note,
        string userName)
    {
        var auditEntries = new List<BuildRecordAudit>();

        AddAuditEntryIfChanged(auditEntries, buildRecord, "WifiSsid", buildRecord.WifiSsid, wifiSsid, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "RouterUsed", buildRecord.RouterUsed, routerUsed, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "Note", buildRecord.Note, note, userName);

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
