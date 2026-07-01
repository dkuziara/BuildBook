using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderNote
{
    public int Id { get; set; }

    public int OrderRecordId { get; set; }

    public OrderRecord? OrderRecord { get; set; }

    public OrderNoteType NoteType { get; set; } = OrderNoteType.InternalNote;

    public string NoteText { get; set; } = string.Empty;

    public int? CreatedByUserId { get; set; }

    public ApplicationUser? CreatedByUser { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? LastUpdatedByUserId { get; set; }

    public ApplicationUser? LastUpdatedByUser { get; set; }

    public DateTimeOffset? LastUpdatedAt { get; set; }
}
