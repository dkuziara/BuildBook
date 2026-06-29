using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class RmaRegisterFilter
{
    public string? Search { get; set; }

    public RmaStatus? Status { get; set; }

    public string? Customer { get; set; }

    public string? Product { get; set; }

    public string? SerialNumber { get; set; }

    public string? AssignedTo { get; set; }

    public RmaPriority? Priority { get; set; }

    public DateOnly? DueDate { get; set; }

    public bool? HasLinkedBuildRecord { get; set; }

    public RmaRegisterSortColumn SortBy { get; set; } = RmaRegisterSortColumn.LastUpdated;

    public bool SortDescending { get; set; } = true;

    public bool HasAnyFilter()
    {
        return !string.IsNullOrWhiteSpace(Search)
            || Status is not null
            || !string.IsNullOrWhiteSpace(Customer)
            || !string.IsNullOrWhiteSpace(Product)
            || !string.IsNullOrWhiteSpace(SerialNumber)
            || !string.IsNullOrWhiteSpace(AssignedTo)
            || Priority is not null
            || DueDate is not null
            || HasLinkedBuildRecord is not null;
    }
}
