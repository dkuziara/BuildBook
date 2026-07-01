namespace BuildBook.Application.Rmas;

public interface IRmaRegisterExcelExporter
{
    Task<byte[]> ExportAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
