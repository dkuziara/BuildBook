namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaIntakeRequest
{
    public string CustomerName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string? ProductCode { get; set; }

    public string? SerialNumber { get; set; }

    public string FaultSummary { get; set; } = string.Empty;

    public string InitialFaultDescription { get; set; } = string.Empty;

    public string? FaultDescription { get; set; }

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerReference { get; set; }

    public string? SupportTicketNumber { get; set; }

    public string? SupportTicketUrl { get; set; }

    public string? OriginalOrderNumber { get; set; }

    public DateOnly? OriginalOrderDate { get; set; }

    public string? OriginalInvoiceNumber { get; set; }
}
