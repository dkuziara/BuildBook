namespace BuildBook.Domain.Rmas;

public static class RmaChecklistTemplate
{
    public static readonly string[] DefaultItems =
    [
        "Diagnose fault",
        "Confirm serial number",
        "Check linked Build Record",
        "Confirm warranty status",
        "Fix issue",
        "Run functional test",
        "Run product-specific test",
        "Check licence key",
        "Check antivirus/security where applicable",
        "Confirm BitLocker/recovery state where applicable",
        "Clean/prepare device",
        "Confirm return address",
        "Confirm shipment approved",
        "Arrange courier/collection",
        "Mark shipped",
        "Close RMA"
    ];
}
