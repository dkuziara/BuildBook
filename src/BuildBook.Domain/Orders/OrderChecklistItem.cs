using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderChecklistItem
{
    public int Id { get; set; }

    public int OrderRecordId { get; set; }

    public OrderRecord? OrderRecord { get; set; }

    public int DisplayOrder { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int? CompletedByUserId { get; set; }

    public ApplicationUser? CompletedByUser { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? Source { get; set; }

    public string? ImportedCompletedText { get; set; }

    public bool ShowInBoardView { get; set; }
}
