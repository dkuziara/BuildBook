namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordAuditHistoryReader
{
    Task<IReadOnlyList<BuildRecordAuditHistoryEntry>> ListByBuildRecordIdAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default);
}
