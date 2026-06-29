namespace BuildBook.Application.Rmas;

public sealed record RmaBuildRecordMatchRequest(
    string? SerialNumber,
    string? ProductCode,
    string? ProductName,
    string? CustomerName);
