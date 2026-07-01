using System.IO.Compression;
using System.Text;
using BuildBook.Application.Customers;
using BuildBook.Infrastructure.Persistence.Customers;

namespace BuildBook.Tests;

public class CustomerListExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesCustomerListColumnsAsExcelWorkbook()
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
                "Device <Test>",
                null,
                "north@example.com",
                null,
                null,
                "No Contract",
                null,
                false,
                new DateTimeOffset(2026, 6, 29, 8, 30, 0, TimeSpan.Zero))
        ]);
        var exporter = new CustomerListExcelExporter(reader);

        var workbook = await exporter.ExportAsync(new CustomerListFilter { SupportContractStatus = "Active" });

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");
        var workbookXml = ReadEntryText(archive, "xl/workbook.xml");

        Assert.Contains("Customers", workbookXml);
        Assert.Contains("Customer name", worksheetXml);
        Assert.Contains("Acme Medical", worksheetXml);
        Assert.Contains("Device &lt;Test&gt;", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
        Assert.Equal("Active", reader.LastFilter?.SupportContractStatus);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        using var stream = archive.GetEntry(entryPath)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
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
