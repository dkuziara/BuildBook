namespace BuildBook.Application.BuildRecords;

public interface IHomePageReader
{
    Task<BuildBookHomePageModel> GetAsync(
        string? userIdentity,
        CancellationToken cancellationToken = default);
}
