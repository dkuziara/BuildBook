namespace BuildBook.Application.Customers;

public sealed class CustomerListFilter
{
    public string? Search { get; set; }

    public int? SupportContractLevelId { get; set; }

    public string? SupportContractStatus { get; set; }

    public bool? IsActive { get; set; }

    public CustomerSortColumn SortBy { get; set; } = CustomerSortColumn.Name;

    public bool SortDescending { get; set; }

    public bool HasAnyFilter()
    {
        return !string.IsNullOrWhiteSpace(Search)
            || SupportContractLevelId is not null
            || !string.IsNullOrWhiteSpace(SupportContractStatus)
            || IsActive is not null;
    }
}
