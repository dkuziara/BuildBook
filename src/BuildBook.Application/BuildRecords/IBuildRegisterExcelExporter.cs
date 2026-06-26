namespace BuildBook.Application.BuildRecords;

public interface IBuildRegisterExcelExporter
{
    Task<byte[]> ExportAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
