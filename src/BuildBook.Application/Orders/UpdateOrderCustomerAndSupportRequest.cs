namespace BuildBook.Application.Orders;

public sealed class UpdateOrderCustomerAndSupportRequest
{
    public string? ProductCode { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerReference { get; set; }

    public string? CustomerPurchaseOrderNumber { get; set; }

    public string? InternalOrderReference { get; set; }

    public string? QuoteNumber { get; set; }

    public string? SupportTicketNo { get; set; }
}
