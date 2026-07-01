using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderRecordId { get; set; }

    public OrderRecord? OrderRecord { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public int? ChangedByUserId { get; set; }

    public ApplicationUser? ChangedByUser { get; set; }

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Reason { get; set; }
}
