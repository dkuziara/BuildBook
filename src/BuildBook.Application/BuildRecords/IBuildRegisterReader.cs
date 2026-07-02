using BuildBook.Application.Paging;

namespace BuildBook.Application.BuildRecords;

public interface IBuildRegisterReader
{
    Task<PagedResult<BuildRegisterRow>> GetPageAsync(
        BuildRegisterFilter? filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BuildRegisterRow>> ListAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
