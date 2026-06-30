namespace BuildBook.Application.Rmas;

public enum RmaReportScope
{
    All = 0,
    OperationalOpen = 1,
    OperationalOverdue = 2,
    OperationalDueThisWeek = 3,
    OperationalWaitingForCustomer = 4,
    OperationalWaitingForParts = 5,
    OperationalReadyToShip = 6,
    OperationalShippedNotClosed = 7,
    Customer = 8,
    Product = 9,
    SerialNumber = 10,
    RepeatReturns = 11,
    FaultCategory = 12,
    RootCauseCategory = 13,
    ProductFaultCombination = 14,
    ChargeableRepairs = 15,
    OutOfWarrantyRepairs = 16,
    AwaitingApproval = 17,
    AwaitingPayment = 18
}
