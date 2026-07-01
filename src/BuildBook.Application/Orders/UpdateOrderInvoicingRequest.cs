namespace BuildBook.Application.Orders;

public sealed class UpdateOrderInvoicingRequest
{
    public bool? ContractReadyForInvoicing { get; set; }

    public DateOnly? ReadyForInvoicingDate { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateOnly? InvoicedDate { get; set; }

    public string? InvoicingNotes { get; set; }
}
