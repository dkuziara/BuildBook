using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed class UpdateOrderWorkflowRequest
{
    public OrderPriority? Priority { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }
}
