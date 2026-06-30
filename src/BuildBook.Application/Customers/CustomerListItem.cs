namespace BuildBook.Application.Customers;

public sealed record CustomerListItem(
    int Id,
    string Name,
    string? PrimaryContactName,
    string? MainEmail,
    string? MainPhone,
    string? SupportContractLevelName,
    string SupportContractStatus,
    DateOnly? SupportContractEndDate,
    bool IsActive,
    DateTimeOffset LastUpdatedAt);
