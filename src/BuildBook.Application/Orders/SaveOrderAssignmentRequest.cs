using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed class SaveOrderAssignmentRequest
{
    public int? ApplicationUserId { get; set; }

    public string? ImportedUserText { get; set; }

    public OrderAssignmentType? AssignmentType { get; set; }
}
