namespace BuildBook.Application.BuildRecords;

public interface IMissingDataReportExcelExporter
{
    Task<byte[]> ExportAsync(
        MissingDataReportType reportType,
        CancellationToken cancellationToken = default);
}
