namespace BuildBook.Application.Rmas;

public interface IRmaRegisterCsvExporter
{
    Task<string> ExportAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
