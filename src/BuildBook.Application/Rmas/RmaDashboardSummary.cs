namespace BuildBook.Application.Rmas;

public sealed record RmaDashboardSummary(
    int OpenCount,
    int OverdueCount,
    int WaitingForCustomerCount,
    int WaitingForPartsCount,
    int ReadyToShipCount,
    int ShippedNotClosedCount);
