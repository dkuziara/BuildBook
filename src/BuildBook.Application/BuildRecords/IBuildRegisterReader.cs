namespace BuildBook.Application.BuildRecords;

public interface IBuildRegisterReader
{
    Task<IReadOnlyList<BuildRegisterRow>> ListAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
