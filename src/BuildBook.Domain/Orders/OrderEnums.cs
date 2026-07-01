namespace BuildBook.Domain.Orders;

public enum OrderPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum OrderAssignmentType
{
    Owner = 1,
    Production = 2,
    Support = 3,
    SalesAdmin = 4,
    Qa = 5,
    Other = 6
}

public enum OrderNoteType
{
    InternalNote = 1,
    ProductionNote = 2,
    ShippingNote = 3,
    InvoicingNote = 4,
    PlannerImportedNote = 5
}

public enum OrderImportWarningSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3
}
