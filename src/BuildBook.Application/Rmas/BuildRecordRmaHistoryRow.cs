using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed record BuildRecordRmaHistoryRow(
    int Id,
    string RmaNumber,
    RmaStatus Status,
    string FaultSummary,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt,
    RmaOutcome? Outcome);
