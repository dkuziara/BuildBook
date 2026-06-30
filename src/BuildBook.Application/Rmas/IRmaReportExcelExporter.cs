namespace BuildBook.Application.Rmas;

public interface IRmaReportExcelExporter
{
    Task<byte[]> ExportAsync(
        RmaReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
