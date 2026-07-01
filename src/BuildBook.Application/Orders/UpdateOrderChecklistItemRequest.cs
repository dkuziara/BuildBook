namespace BuildBook.Application.Orders;

public sealed class UpdateOrderChecklistItemRequest
{
    public int? ChecklistItemId { get; set; }

    public bool? IsCompleted { get; set; }

    public string? Text { get; set; }
}
