using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderAssignment
{
    public int Id { get; set; }

    public int OrderRecordId { get; set; }

    public OrderRecord? OrderRecord { get; set; }

    public int? ApplicationUserId { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

    public string? ImportedUserText { get; set; }

    public OrderAssignmentType? AssignmentType { get; set; }

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? AssignedByUserId { get; set; }

    public ApplicationUser? AssignedByUser { get; set; }
}
