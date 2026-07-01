namespace BuildBook.Domain.Orders;

public sealed class OrderLabel
{
    public int Id { get; set; }

    public int OrderRecordId { get; set; }

    public OrderRecord? OrderRecord { get; set; }

    public string LabelText { get; set; } = string.Empty;

    public string? Source { get; set; }
}
