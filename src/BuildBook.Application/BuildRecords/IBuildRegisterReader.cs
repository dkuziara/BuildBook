namespace BuildBook.Application.BuildRecords;

public interface IBuildRegisterReader
{
    Task<IReadOnlyList<BuildRegisterRow>> ListAsync(
        CancellationToken cancellationToken = default);
}
