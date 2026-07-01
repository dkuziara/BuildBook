using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed class OrderRegisterFilter
{
    public OrderRegisterSortColumn SortBy { get; set; } = OrderRegisterSortColumn.LastUpdated;

    public bool SortDescending { get; set; } = true;

    public string? Search { get; set; }

    public string? Customer { get; set; }

    public string? AssignedTo { get; set; }

    public string? Status { get; set; }

    public OrderPriority? Priority { get; set; }

    public DateOnly? DueDate { get; set; }

    public bool? IsOverdue { get; set; }

    public bool? IsCompleted { get; set; }

    public bool? HasLinkedBuildRecord { get; set; }

    public bool HasAnyFilter()
    {
        return !string.IsNullOrWhiteSpace(Search)
            || !string.IsNullOrWhiteSpace(Customer)
            || !string.IsNullOrWhiteSpace(AssignedTo)
            || !string.IsNullOrWhiteSpace(Status)
            || Priority is not null
            || DueDate is not null
            || IsOverdue is not null
            || IsCompleted is not null
            || HasLinkedBuildRecord is not null;
    }
}
