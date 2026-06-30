namespace BuildBook.Application.Customers;

public sealed record LinkedCustomerBuildRecord(
    int Id,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? MachineName,
    string? RadSightVersion,
    DateOnly? DateShipped,
    DateTimeOffset LastUpdatedAt);
