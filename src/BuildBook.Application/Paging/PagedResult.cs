using System.Linq;

namespace BuildBook.Application.Paging;

public sealed class PagedResult<T>
{
    public PagedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        Items = items;
        TotalCount = Math.Max(0, totalCount);
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Max(1, pageSize);
    }

    public IReadOnlyList<T> Items { get; }

    public int TotalCount { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public int StartRecord => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

    public int EndRecord => TotalCount == 0 ? 0 : StartRecord + Items.Count - 1;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(
        IReadOnlyList<T> allItems,
        int pageNumber,
        int pageSize)
    {
        var normalizedPageSize = Math.Max(1, pageSize);
        var totalCount = allItems.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)normalizedPageSize));
        var normalizedPageNumber = Math.Min(Math.Max(1, pageNumber), totalPages);
        var items = allItems
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToArray();

        return new PagedResult<T>(items, totalCount, normalizedPageNumber, normalizedPageSize);
    }
}
