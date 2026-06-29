using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed record RmaStatusHistoryEntry(
    int Id,
    RmaStatus? OldStatus,
    RmaStatus NewStatus,
    string ChangedBy,
    DateTimeOffset ChangedAt,
    string? Reason);
