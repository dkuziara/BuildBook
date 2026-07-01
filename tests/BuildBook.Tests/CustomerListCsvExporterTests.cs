using BuildBook.Application.Customers;
using BuildBook.Infrastructure.Persistence.Customers;

namespace BuildBook.Tests;

public class CustomerListCsvExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesCustomerListColumnsAsCsv()
    {
        var reader = new StubCustomerListReader(
        [
            new CustomerListItem(
                1,
                "Acme Medical",
                "Jane Smith",
                "ops@example.com",
                "01234 567890",
                "Gold",
                "Active",
                new DateOnly(2026, 7, 31),
                true,
                new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero)),
            new CustomerListItem(
                2,
                "North \"Lab\"",
                null,
                null,
                null,
                null,
                "No Contract",
                null,
                false,
                new DateTimeOffset(2026, 6, 29, 8, 30, 0, TimeSpan.Zero))
        ]);
        var exporter = new CustomerListCsvExporter(reader);

        var csv = await exporter.ExportAsync(new CustomerListFilter { Search = "Acme" });

        Assert.Contains("Customer name,Primary contact,Email,Phone,Support contract level,Support contract status,Contract end date,Active,Last updated", csv);
        Assert.Contains("Acme Medical,Jane Smith,ops@example.com,01234 567890,Gold,Active,31 Jul 2026,Active,", csv);
        Assert.Contains("\"North \"\"Lab\"\"\",Not recorded,Not recorded,Not recorded,Not recorded,No Contract,Not recorded,Inactive,", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
        Assert.Equal("Acme", reader.LastFilter?.Search);
    }

    private sealed class StubCustomerListReader(IReadOnlyList<CustomerListItem> rows) : ICustomerListReader
    {
        public CustomerListFilter? LastFilter { get; private set; }

        public Task<IReadOnlyList<CustomerListItem>> ListAsync(
            CustomerListFilter filter,
            CancellationToken cancellationToken = default)
        {
            LastFilter = filter;
            return Task.FromResult(rows);
        }
    }
}
