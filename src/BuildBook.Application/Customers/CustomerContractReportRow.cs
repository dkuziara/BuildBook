namespace BuildBook.Application.Customers;

public sealed record CustomerContractReportRow(
    int CustomerId,
    string CustomerName,
    string? AccountCode,
    string? PrimaryContactName,
    string? MainEmail,
    string? MainPhone,
    string? SupportContractLevelName,
    string SupportContractStatus,
    DateOnly? SupportContractEndDate,
    bool IsActive,
    int BuildRecordCount,
    int LinkedRmaCount,
    int OpenRmaCount,
    int OverdueRmaCount,
    DateTimeOffset LastUpdatedAt);
