namespace BuildBook.Application.BuildRecords;

public interface IMissingDataReportCsvExporter
{
    Task<string> ExportAsync(
        MissingDataReportType reportType,
        CancellationToken cancellationToken = default);
}
