namespace BuildBook.Application.BuildRecords;

public interface IBuildRegisterCsvExporter
{
    Task<string> ExportAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
