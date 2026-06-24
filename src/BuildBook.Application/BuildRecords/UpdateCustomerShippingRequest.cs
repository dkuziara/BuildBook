namespace BuildBook.Application.BuildRecords;

public sealed class UpdateCustomerShippingRequest
{
    public int? CustomerId { get; set; }

    public string? CustomerOrder { get; set; }

    public string? OANumber { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateOnly? DateShipped { get; set; }
}
