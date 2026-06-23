namespace BuildBook.Domain.BuildRecords;

public enum AuditAction
{
    Created = 0,
    Updated = 1,
    SensitiveValueViewed = 2,
    SensitiveValueChanged = 3,
    Retired = 4,
    ImportPerformed = 5
}
