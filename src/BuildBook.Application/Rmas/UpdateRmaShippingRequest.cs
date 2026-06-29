namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaShippingRequest
{
    public string? ReturnMethod { get; set; }

    public string? Courier { get; set; }

    public string? TrackingNumber { get; set; }

    public bool? CollectionArranged { get; set; }

    public DateOnly? CollectionDate { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public string? ShippedBy { get; set; }

    public string? ReturnAddress { get; set; }

    public string? ShippingNotes { get; set; }

    public bool? ProofOfDeliveryReceived { get; set; }

    public DateOnly? ProofOfDeliveryDate { get; set; }
}
