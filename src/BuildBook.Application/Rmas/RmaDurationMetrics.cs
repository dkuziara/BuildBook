namespace BuildBook.Application.Rmas;

public sealed record RmaDurationMetrics(
    DateTimeOffset CurrentStatusSince,
    int DaysOpen,
    int DaysInCurrentStatus,
    int DaysOnHold,
    int? RepairDays,
    int? ReadyToShipToShippedDays);
