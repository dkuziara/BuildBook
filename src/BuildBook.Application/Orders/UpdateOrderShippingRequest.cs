namespace BuildBook.Application.Orders;

public sealed class UpdateOrderShippingRequest
{
    public bool? ShippingRequired { get; set; }

    public string? ShippingMethod { get; set; }

    public string? Courier { get; set; }

    public string? TrackingNumber { get; set; }

    public bool? CollectionRequired { get; set; }

    public DateOnly? CollectionDate { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public string? ShippingNotes { get; set; }
}
