using BuildBook.Application.Paging;

namespace BuildBook.Tests;

public class PagedResultTests
{
    [Fact]
    public void Create_SlicesItemsAndCalculatesMetadata()
    {
        var result = PagedResult<int>.Create(Enumerable.Range(1, 12).ToArray(), 2, 5);

        Assert.Equal([6, 7, 8, 9, 10], result.Items);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(6, result.StartRecord);
        Assert.Equal(10, result.EndRecord);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void Create_ClampsPageNumberWhenItExceedsAvailablePages()
    {
        var result = PagedResult<int>.Create([1, 2, 3], 9, 2);

        Assert.Equal([3], result.Items);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasNextPage);
    }
}
