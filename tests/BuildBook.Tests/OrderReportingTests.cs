using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using BuildBook.Infrastructure.Persistence.Orders;
using System.Reflection;

namespace BuildBook.Tests;

public class OrderReportingTests
{
    [Fact]
    public void OrderBoardReader_BuildWarnings_ReturnsExpectedReadinessWarnings()
    {
        var buildWarningsMethod = typeof(OrderBoardReader).GetMethod(
            "BuildWarnings",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildWarningsMethod);

        var warnings = (IReadOnlyList<string>?)buildWarningsMethod!.Invoke(
            null,
            [
                BuildBookOrderStatuses.Shipped,
                Array.Empty<string>(),
                1,
                2,
                0,
                false,
                null,
                null,
                null,
                null
            ]);

        Assert.NotNull(warnings);
        Assert.Contains("No assignments", warnings!);
        Assert.Contains("Checklist incomplete", warnings);
        Assert.Contains("No linked Build Record", warnings);
        Assert.Contains("Shipped date missing", warnings);
    }

    [Fact]
    public void OrderReportReader_ApplyFilter_ReturnsReadyForInvoicingAndMissingInvoiceRows()
    {
        var applyFilterMethod = typeof(OrderReportReader).GetMethod(
            "ApplyFilter",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(applyFilterMethod);

        var today = new DateOnly(2026, 7, 1);
        var monthStart = new DateOnly(2026, 7, 1);
        var monthEnd = new DateOnly(2026, 7, 31);
        IReadOnlyList<OrderReportRow> rows =
        [
            new OrderReportRow(
                1,
                "ORD-0100",
                "Ready order",
                BuildBookOrderStatuses.ContractReadyForInvoicing,
                "Acme Medical",
                OrderPriority.High,
                "Planner",
                today,
                2,
                2,
                1,
                true,
                today,
                null,
                null,
                today.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(-5),
                DateTimeOffset.UtcNow,
                false),
            new OrderReportRow(
                2,
                "ORD-0101",
                "Invoiced missing number",
                BuildBookOrderStatuses.Invoiced,
                "Acme Medical",
                OrderPriority.High,
                "Planner",
                today,
                2,
                2,
                1,
                true,
                today.AddDays(-2),
                null,
                today,
                today.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(-5),
                DateTimeOffset.UtcNow,
                false),
            new OrderReportRow(
                3,
                "ORD-0102",
                "Invoiced complete",
                BuildBookOrderStatuses.Invoiced,
                "Acme Medical",
                OrderPriority.High,
                "Planner",
                today,
                2,
                2,
                1,
                true,
                today.AddDays(-2),
                "INV-123",
                today,
                today.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(-5),
                DateTimeOffset.UtcNow,
                false)
        ];

        var readyRows = (IReadOnlyList<OrderReportRow>?)applyFilterMethod!.Invoke(
            null,
            [rows, new OrderReportFilter { Scope = OrderReportScope.OrdersReadyForInvoicing }, today, monthStart, monthEnd]);
        var missingInvoiceRows = (IReadOnlyList<OrderReportRow>?)applyFilterMethod.Invoke(
            null,
            [rows, new OrderReportFilter { Scope = OrderReportScope.MissingInvoiceNumber }, today, monthStart, monthEnd]);

        Assert.NotNull(readyRows);
        Assert.NotNull(missingInvoiceRows);
        Assert.Single(readyRows!);
        Assert.Equal("ORD-0100", readyRows[0].OrderNumber);
        Assert.Single(missingInvoiceRows!);
        Assert.Equal("ORD-0101", missingInvoiceRows[0].OrderNumber);
    }
}
