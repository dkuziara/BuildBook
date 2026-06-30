using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaReportReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IRmaReportReader
{
    public async Task<IReadOnlyList<RmaReportRow>> ListAsync(
        RmaReportFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var rawRows = await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.IsActive)
            .OrderByDescending(rmaRecord => rmaRecord.LastUpdatedAt)
            .ThenBy(rmaRecord => rmaRecord.RmaNumber)
            .Select(rmaRecord => new RawRmaReportRow(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.Customer == null ? null : rmaRecord.Customer.Name,
                rmaRecord.ProductName,
                rmaRecord.ProductCode,
                rmaRecord.SerialNumber,
                rmaRecord.FaultSummary,
                rmaRecord.FaultCategory,
                rmaRecord.RootCauseCategory,
                rmaRecord.WarrantyStatus,
                rmaRecord.ChargeableRepair,
                rmaRecord.CustomerApprovalRequired,
                rmaRecord.CustomerApprovalReceived,
                rmaRecord.PurchaseOrderNumber,
                rmaRecord.RepairInvoiceNumber,
                rmaRecord.EstimatedRepairCost,
                rmaRecord.ActualRepairCost,
                rmaRecord.AssignedTo,
                rmaRecord.DueDate,
                rmaRecord.OnHoldReason,
                rmaRecord.DateItemReceived,
                rmaRecord.RepairCompletedDate,
                rmaRecord.ShippedDate,
                rmaRecord.CreatedAt,
                rmaRecord.LastUpdatedAt,
                rmaRecord.ClosedAt,
                rmaRecord.BuildRecordId))
            .ToListAsync(cancellationToken);

        var recordIds = rawRows.Select(row => row.Id).ToArray();
        var statusHistory = await dbContext.RmaStatusHistory
            .AsNoTracking()
            .Where(entry => recordIds.Contains(entry.RmaRecordId))
            .OrderBy(entry => entry.ChangedAt)
            .ThenBy(entry => entry.Id)
            .Select(entry => new RawStatusHistoryEntry(
                entry.Id,
                entry.RmaRecordId,
                entry.NewStatus,
                entry.ChangedAt))
            .ToListAsync(cancellationToken);

        var statusHistoryLookup = statusHistory
            .GroupBy(entry => entry.RmaRecordId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<RawStatusHistoryEntry>)group.ToList());

        var reportRows = rawRows
            .Select(row =>
            {
                var historyEntries = statusHistoryLookup.TryGetValue(row.Id, out var entries)
                    ? entries
                    : [];
                var metrics = BuildMetrics(row, historyEntries, now);

                return new RmaReportRow(
                    row.Id,
                    row.RmaNumber,
                    row.Status,
                    row.CustomerName,
                    row.ProductName,
                    row.ProductCode,
                    row.SerialNumber,
                    row.FaultSummary,
                    row.FaultCategory,
                    row.RootCauseCategory,
                    row.WarrantyStatus,
                    row.ChargeableRepair,
                    row.CustomerApprovalRequired,
                    row.CustomerApprovalReceived,
                    row.PurchaseOrderNumber,
                    row.RepairInvoiceNumber,
                    row.EstimatedRepairCost,
                    row.ActualRepairCost,
                    row.AssignedTo,
                    row.DueDate,
                    row.OnHoldReason,
                    row.DateItemReceived,
                    row.RepairCompletedDate,
                    row.ShippedDate,
                    row.CreatedAt,
                    row.LastUpdatedAt,
                    row.ClosedAt,
                    CountPreviousRmas(rawRows, row),
                    metrics.CurrentStatusSince,
                    metrics.DaysOpen,
                    metrics.DaysInCurrentStatus,
                    metrics.DaysOnHold,
                    metrics.RepairDays,
                    metrics.ReadyToShipToShippedDays);
            })
            .ToList();

        return ApplyFilter(reportRows, filter, today);
    }

    public async Task<RmaDurationMetrics?> GetMetricsAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var row = await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.Id == rmaRecordId && rmaRecord.IsActive)
            .Select(rmaRecord => new RawRmaReportRow(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.Customer == null ? null : rmaRecord.Customer.Name,
                rmaRecord.ProductName,
                rmaRecord.ProductCode,
                rmaRecord.SerialNumber,
                rmaRecord.FaultSummary,
                rmaRecord.FaultCategory,
                rmaRecord.RootCauseCategory,
                rmaRecord.WarrantyStatus,
                rmaRecord.ChargeableRepair,
                rmaRecord.CustomerApprovalRequired,
                rmaRecord.CustomerApprovalReceived,
                rmaRecord.PurchaseOrderNumber,
                rmaRecord.RepairInvoiceNumber,
                rmaRecord.EstimatedRepairCost,
                rmaRecord.ActualRepairCost,
                rmaRecord.AssignedTo,
                rmaRecord.DueDate,
                rmaRecord.OnHoldReason,
                rmaRecord.DateItemReceived,
                rmaRecord.RepairCompletedDate,
                rmaRecord.ShippedDate,
                rmaRecord.CreatedAt,
                rmaRecord.LastUpdatedAt,
                rmaRecord.ClosedAt,
                rmaRecord.BuildRecordId))
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var historyEntries = await dbContext.RmaStatusHistory
            .AsNoTracking()
            .Where(entry => entry.RmaRecordId == rmaRecordId)
            .OrderBy(entry => entry.ChangedAt)
            .ThenBy(entry => entry.Id)
            .Select(entry => new RawStatusHistoryEntry(
                entry.Id,
                entry.RmaRecordId,
                entry.NewStatus,
                entry.ChangedAt))
            .ToListAsync(cancellationToken);

        return BuildMetrics(row, historyEntries, DateTimeOffset.UtcNow);
    }

    private static IReadOnlyList<RmaReportRow> ApplyFilter(
        IReadOnlyList<RmaReportRow> rows,
        RmaReportFilter? filter,
        DateOnly today)
    {
        if (filter is null)
        {
            return rows;
        }

        var value = Normalize(filter.Value);

        return filter.Scope switch
        {
            RmaReportScope.OperationalOpen => rows.Where(row => IsOperationallyOpen(row.Status)).ToList(),
            RmaReportScope.OperationalOverdue => rows.Where(row =>
                IsOperationallyOpen(row.Status)
                && row.DueDate is not null
                && row.DueDate < today).ToList(),
            RmaReportScope.OperationalDueThisWeek => rows.Where(row =>
                IsOperationallyOpen(row.Status)
                && row.DueDate is not null
                && row.DueDate >= today
                && row.DueDate <= today.AddDays(7)).ToList(),
            RmaReportScope.OperationalWaitingForCustomer => rows.Where(row =>
                row.Status == RmaStatus.OnHold
                && string.Equals(row.OnHoldReason, RmaOnHoldReasons.WaitingForCustomer, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.OperationalWaitingForParts => rows.Where(row =>
                row.Status == RmaStatus.OnHold
                && string.Equals(row.OnHoldReason, RmaOnHoldReasons.WaitingForParts, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.OperationalReadyToShip => rows.Where(row => row.Status == RmaStatus.ReadyToShip).ToList(),
            RmaReportScope.OperationalShippedNotClosed => rows.Where(row => row.Status == RmaStatus.Shipped).ToList(),
            RmaReportScope.Customer when value is not null => rows.Where(row =>
                string.Equals(Normalize(row.CustomerName), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.Product when value is not null => rows.Where(row =>
                string.Equals(Normalize(row.ProductName), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.SerialNumber when value is not null => rows.Where(row =>
                string.Equals(Normalize(row.SerialNumber), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.RepeatReturns => rows.Where(row => row.PreviousRmaCount > 0).ToList(),
            RmaReportScope.FaultCategory when value is not null => rows.Where(row =>
                string.Equals(row.FaultCategory?.ToString(), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.RootCauseCategory when value is not null => rows.Where(row =>
                string.Equals(row.RootCauseCategory?.ToString(), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.ProductFaultCombination when value is not null => rows.Where(row =>
                string.Equals(BuildProductFaultKey(row.ProductName, row.FaultCategory), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            RmaReportScope.ChargeableRepairs => rows.Where(row => row.ChargeableRepair == true).ToList(),
            RmaReportScope.OutOfWarrantyRepairs => rows.Where(row => row.WarrantyStatus == RmaWarrantyStatus.OutOfWarranty).ToList(),
            RmaReportScope.AwaitingApproval => rows.Where(row =>
                row.CustomerApprovalRequired == true
                && row.CustomerApprovalReceived != true).ToList(),
            RmaReportScope.AwaitingPayment => rows.Where(row =>
                row.ChargeableRepair == true
                && row.CustomerApprovalReceived == true
                && string.IsNullOrWhiteSpace(row.PurchaseOrderNumber)
                && string.IsNullOrWhiteSpace(row.RepairInvoiceNumber)).ToList(),
            _ => rows
        };
    }

    private static RmaDurationMetrics BuildMetrics(
        RawRmaReportRow row,
        IReadOnlyList<RawStatusHistoryEntry> statusHistory,
        DateTimeOffset now)
    {
        var end = row.ClosedAt ?? now;
        var currentStatusSince = statusHistory.Count > 0
            ? statusHistory[^1].ChangedAt
            : row.CreatedAt;
        var daysOnHold = 0;

        for (var index = 0; index < statusHistory.Count; index++)
        {
            var historyEntry = statusHistory[index];
            if (historyEntry.NewStatus != RmaStatus.OnHold)
            {
                continue;
            }

            var holdEnd = index + 1 < statusHistory.Count
                ? statusHistory[index + 1].ChangedAt
                : end;
            daysOnHold += CountWholeDays(historyEntry.ChangedAt, holdEnd);
        }

        var repairStartDate = row.DateItemReceived ?? DateOnly.FromDateTime(row.CreatedAt.UtcDateTime);
        int? repairDays = row.RepairCompletedDate is null
            ? null
            : Math.Max(0, row.RepairCompletedDate.Value.DayNumber - repairStartDate.DayNumber);
        var readyToShipAt = statusHistory
            .LastOrDefault(entry => entry.NewStatus == RmaStatus.ReadyToShip)
            ?.ChangedAt;
        int? readyToShipToShippedDays = readyToShipAt is null || row.ShippedDate is null
            ? null
            : Math.Max(0, row.ShippedDate.Value.DayNumber - DateOnly.FromDateTime(readyToShipAt.Value.UtcDateTime).DayNumber);

        return new RmaDurationMetrics(
            currentStatusSince,
            CountWholeDays(row.CreatedAt, end),
            CountWholeDays(currentStatusSince, end),
            daysOnHold,
            repairDays,
            readyToShipToShippedDays);
    }

    private static int CountPreviousRmas(
        IReadOnlyList<RawRmaReportRow> rows,
        RawRmaReportRow row)
    {
        return rows.Count(other =>
            other.Id != row.Id
            && ((row.BuildRecordId is not null && other.BuildRecordId == row.BuildRecordId)
                || (Normalize(row.SerialNumber) is { } serialNumber
                    && string.Equals(Normalize(other.SerialNumber), serialNumber, StringComparison.OrdinalIgnoreCase))));
    }

    private static int CountWholeDays(DateTimeOffset start, DateTimeOffset end)
    {
        var startDate = DateOnly.FromDateTime(start.UtcDateTime);
        var endDate = DateOnly.FromDateTime(end.UtcDateTime);
        return Math.Max(0, endDate.DayNumber - startDate.DayNumber);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsOperationallyOpen(RmaStatus status)
    {
        return status != RmaStatus.Closed
            && status != RmaStatus.CancelledNoReply
            && status != RmaStatus.CustomerFixed;
    }

    private static string BuildProductFaultKey(string productName, RmaFaultCategory? faultCategory)
    {
        return $"{Normalize(productName)}||{faultCategory?.ToString() ?? "None"}";
    }

    private sealed record RawRmaReportRow(
        int Id,
        string RmaNumber,
        RmaStatus Status,
        string? CustomerName,
        string ProductName,
        string? ProductCode,
        string? SerialNumber,
        string FaultSummary,
        RmaFaultCategory? FaultCategory,
        RmaRootCauseCategory? RootCauseCategory,
        RmaWarrantyStatus? WarrantyStatus,
        bool? ChargeableRepair,
        bool? CustomerApprovalRequired,
        bool? CustomerApprovalReceived,
        string? PurchaseOrderNumber,
        string? RepairInvoiceNumber,
        decimal? EstimatedRepairCost,
        decimal? ActualRepairCost,
        string? AssignedTo,
        DateOnly? DueDate,
        string? OnHoldReason,
        DateOnly? DateItemReceived,
        DateOnly? RepairCompletedDate,
        DateOnly? ShippedDate,
        DateTimeOffset CreatedAt,
        DateTimeOffset LastUpdatedAt,
        DateTimeOffset? ClosedAt,
        int? BuildRecordId);

    private sealed record RawStatusHistoryEntry(
        int Id,
        int RmaRecordId,
        RmaStatus NewStatus,
        DateTimeOffset ChangedAt);
}
