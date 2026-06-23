namespace BuildBook.Domain.BuildRecords;

public enum ImportStatus
{
    Pending = 0,
    Completed = 1,
    CompletedWithWarnings = 2,
    Failed = 3
}
