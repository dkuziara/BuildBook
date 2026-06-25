namespace BuildBook.Application.BuildRecords;

public interface IImportHistoryReader
{
    Task<IReadOnlyList<ImportHistoryEntry>> ListAsync(
        CancellationToken cancellationToken = default);
}
