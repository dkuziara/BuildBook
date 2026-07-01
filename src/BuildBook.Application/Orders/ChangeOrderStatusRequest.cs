namespace BuildBook.Application.Orders;

public sealed class ChangeOrderStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public bool IgnoreReadinessWarnings { get; set; }
}
