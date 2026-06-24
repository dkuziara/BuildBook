namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordSearchService
{
    Task<IReadOnlyList<BuildRecordSearchResult>> SearchAsync(
        string? searchText,
        CancellationToken cancellationToken = default);
}
