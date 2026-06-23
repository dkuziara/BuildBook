namespace BuildBook.Domain.BuildRecords;

public enum InternalStatus
{
    Draft = 0,
    InBuild = 1,
    AwaitingCheck = 2,
    Checked = 3,
    Shipped = 4,
    Returned = 5,
    Retired = 6
}
