namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaChecklistItemRequest
{
    public int? ChecklistItemId { get; set; }

    public bool? IsCompleted { get; set; }

    public string? Text { get; set; }
}
