namespace BuildBook.Application.BuildRecords;

public interface IMissingDataReportReader
{
    Task<IReadOnlyList<MissingDataReportRow>> ListActiveAsync(
        CancellationToken cancellationToken = default);
}
