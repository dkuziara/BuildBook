namespace BuildBook.Application.Rmas;

public sealed record RmaBuildRecordMatchSuggestion(
    int BuildRecordId,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? CustomerName,
    string? MachineName,
    DateOnly? DateShipped,
    IReadOnlyList<string> MatchReasons);
