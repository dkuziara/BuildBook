using BuildBook.Application.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Customers;

namespace BuildBook.Tests;

public class CustomerReportCsvExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesCustomerContractRowsAsCsv()
    {
        var reader = new StubCustomerReportReader(
            [
                new CustomerContractReportRow(
                    1,
                    "Acme Medical",
                    "AC-100",
                    "Jane Smith",
                    "ops@example.com",
                    "01234",
                    "Gold",
                    "Active",
                    new DateOnly(2026, 7, 31),
                    true,
                    4,
                    3,
                    2,
                    1,
                    new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero))
            ],
            []);
        var exporter = new CustomerReportCsvExporter(reader);

        var csv = await exporter.ExportAsync(new CustomerReportFilter
        {
            Scope = CustomerReportScope.CustomersByContractLevel,
            Value = "Gold"
        });

        Assert.Contains("Customer,Account code,Primary contact,Email,Phone,Support contract level", csv);
        Assert.Contains("Acme Medical,AC-100,Jane Smith,ops@example.com,01234,Gold,Active,2026-07-31,4,3,2,1,", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
        Assert.Equal(CustomerReportScope.CustomersByContractLevel, reader.LastCustomerFilter?.Scope);
    }

    [Fact]
    public async Task ExportAsync_WritesCustomerRmaRowsAsCsv()
    {
        var reader = new StubCustomerReportReader(
            [],
            [
                new CustomerSupportRmaReportRow(
                    10,
                    "RMA-0010",
                    RmaStatus.WorkInProgress,
                    "North \"Lab\"",
                    "Gold",
                    "Active",
                    RmaPriority.Medium,
                    RmaPriority.High,
                    "Device, Test",
                    "SN-1000",
                    "Boot fault",
                    null,
                    new DateOnly(2026, 6, 30),
                    new DateTimeOffset(2026, 6, 30, 12, 0, 0, TimeSpan.Zero),
                    true,
                    true,
                    true)
            ]);
        var exporter = new CustomerReportCsvExporter(reader);

        var csv = await exporter.ExportAsync(new CustomerReportFilter
        {
            Scope = CustomerReportScope.PriorityMismatch
        });

        Assert.Contains("RMA number,Status,Customer,Support contract level", csv);
        Assert.Contains("RMA-0010,Work In Progress,\"North \"\"Lab\"\"\",Gold,Active,Medium,High,\"Device, Test\",SN-1000,Boot fault,,2026-06-30,", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
        Assert.Equal(CustomerReportScope.PriorityMismatch, reader.LastRmaFilter?.Scope);
    }

    private sealed class StubCustomerReportReader(
        IReadOnlyList<CustomerContractReportRow> customerRows,
        IReadOnlyList<CustomerSupportRmaReportRow> rmaRows) : ICustomerReportReader
    {
        public CustomerReportFilter? LastCustomerFilter { get; private set; }
        public CustomerReportFilter? LastRmaFilter { get; private set; }

        public Task<IReadOnlyList<CustomerContractReportRow>> ListCustomersAsync(CustomerReportFilter? filter = null, CancellationToken cancellationToken = default)
        {
            LastCustomerFilter = filter;
            return Task.FromResult(customerRows);
        }

        public Task<IReadOnlyList<CustomerSupportRmaReportRow>> ListRmasAsync(CustomerReportFilter? filter = null, CancellationToken cancellationToken = default)
        {
            LastRmaFilter = filter;
            return Task.FromResult(rmaRows);
        }
    }
}
