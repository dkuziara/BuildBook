namespace BuildBook.Application.Rmas;

public interface IRmaReportCsvExporter
{
    Task<string> ExportAsync(
        RmaReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
