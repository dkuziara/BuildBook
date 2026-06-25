namespace BuildBook.Application.BuildRecords;

public sealed class BuildRegisterFilter
{
    public string? Customer { get; set; }

    public string? ProductCode { get; set; }

    public DateOnly? DateShipped { get; set; }

    public string? RadSightVersion { get; set; }

    public string? WindowsVersion { get; set; }

    public bool HasAnyFilter()
    {
        return !string.IsNullOrWhiteSpace(Customer)
            || !string.IsNullOrWhiteSpace(ProductCode)
            || DateShipped is not null
            || !string.IsNullOrWhiteSpace(RadSightVersion)
            || !string.IsNullOrWhiteSpace(WindowsVersion);
    }
}
