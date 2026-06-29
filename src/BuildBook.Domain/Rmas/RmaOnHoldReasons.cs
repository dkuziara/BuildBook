namespace BuildBook.Domain.Rmas;

public static class RmaOnHoldReasons
{
    public const string WaitingForCustomer = "Waiting for customer";
    public const string WaitingForParts = "Waiting for parts";
    public const string WaitingForPayment = "Waiting for payment";
    public const string WaitingForApproval = "Waiting for approval";
    public const string WaitingForTestEquipment = "Waiting for test equipment";
    public const string WaitingForInternalDecision = "Waiting for internal decision";
    public const string WaitingForCourier = "Waiting for courier";
    public const string Other = "Other";

    public static readonly string[] All =
    [
        WaitingForCustomer,
        WaitingForParts,
        WaitingForPayment,
        WaitingForApproval,
        WaitingForTestEquipment,
        WaitingForInternalDecision,
        WaitingForCourier,
        Other
    ];
}
