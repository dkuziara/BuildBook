namespace BuildBook.Domain.Customers;

public static class CustomerSupportContractStatuses
{
    public const string NoContract = "No Contract";
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string PendingRenewal = "Pending Renewal";
    public const string Suspended = "Suspended";
    public const string Unknown = "Unknown";

    public static readonly string[] All =
    [
        NoContract,
        Active,
        Expired,
        PendingRenewal,
        Suspended,
        Unknown
    ];
}
