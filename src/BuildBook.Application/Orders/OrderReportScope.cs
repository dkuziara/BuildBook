namespace BuildBook.Application.Orders;

public enum OrderReportScope
{
    All = 0,
    OperationalOpen = 1,
    OperationalOverdue = 2,
    OperationalDueThisWeek = 3,
    Status = 4,
    AssignedUser = 5,
    WaitingForPartsStock = 6,
    BuiltNotPreparedForShipping = 7,
    ReadyForCollectionNotShipped = 8,
    ShippedNotReadyForInvoicing = 9,
    ReadyForInvoicingNotInvoiced = 10,
    Customer = 11,
    OrdersWithoutLinkedCustomer = 12,
    NoLinkedBuildRecord = 13,
    MultipleLinkedBuildRecords = 14,
    IncompleteChecklist = 15,
    ReadyBasedOnChecklist = 16,
    OrdersReadyForInvoicing = 17,
    OrdersInvoicedThisMonth = 18,
    OrdersShippedNotInvoiced = 19,
    MissingInvoiceNumber = 20
}
