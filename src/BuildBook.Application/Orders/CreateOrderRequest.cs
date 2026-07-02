using System.ComponentModel.DataAnnotations;
using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed class CreateOrderRequest
{
    [Required(ErrorMessage = "Order title is required.")]
    public string OrderTitle { get; set; } = string.Empty;

    public string? ProductCode { get; set; }

    public string? OrderDescription { get; set; }

    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = BuildBookOrderStatuses.OrderReceived;

    [Required(ErrorMessage = "Priority is required.")]
    public OrderPriority? Priority { get; set; } = OrderPriority.Medium;

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public string? CustomerReference { get; set; }

    public string? CustomerPurchaseOrderNumber { get; set; }

    public string? InternalOrderReference { get; set; }

    public string? QuoteNumber { get; set; }

    public string? SupportTicketNo { get; set; }

    public bool IsRecurring { get; set; }
}
