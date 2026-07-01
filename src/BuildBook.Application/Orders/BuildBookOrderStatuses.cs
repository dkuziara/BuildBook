namespace BuildBook.Application.Orders;

public static class BuildBookOrderStatuses
{
    public const string OrderReceived = "Order Received";
    public const string PartsOrderedOrStockAllocated = "Parts Ordered / Stock Allocated";
    public const string Built = "Built";
    public const string PreparedForShipping = "Prepared for Shipping";
    public const string ReadyForCollection = "Ready for Collection";
    public const string Shipped = "Shipped";
    public const string ContractReadyForInvoicing = "Contract Ready for Invoicing";
    public const string Invoiced = "Invoiced";

    public static readonly string[] DefaultWorkflow =
    [
        OrderReceived,
        PartsOrderedOrStockAllocated,
        Built,
        PreparedForShipping,
        ReadyForCollection,
        Shipped,
        ContractReadyForInvoicing,
        Invoiced
    ];

    public static string SerializeDefaultWorkflow()
    {
        return string.Join(Environment.NewLine, DefaultWorkflow);
    }
}
