namespace BuildBook.Application.Customers;

public sealed class CustomerReportFilter
{
    public CustomerReportScope Scope { get; set; } = CustomerReportScope.AllCustomers;

    public string? Value { get; set; }
}
