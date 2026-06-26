namespace BuildBook.Application.BuildRecords;

public sealed class BuildRegisterFilter
{
    public BuildRegisterSortColumn SortBy { get; set; } = BuildRegisterSortColumn.LastUpdated;

    public bool SortDescending { get; set; } = true;

    public int? CustomerId { get; set; }

    public string? Customer { get; set; }

    public string? ProductCode { get; set; }

    public DateOnly? DateShipped { get; set; }

    public string? RadSightVersion { get; set; }

    public string? WindowsVersion { get; set; }

    public bool HasAnyFilter()
    {
        return CustomerId is not null
            || !string.IsNullOrWhiteSpace(Customer)
            || !string.IsNullOrWhiteSpace(ProductCode)
            || DateShipped is not null
            || !string.IsNullOrWhiteSpace(RadSightVersion)
            || !string.IsNullOrWhiteSpace(WindowsVersion);
    }
}
