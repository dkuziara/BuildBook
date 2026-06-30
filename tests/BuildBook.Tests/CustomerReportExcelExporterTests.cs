using System.IO.Compression;
using System.Text;
using BuildBook.Application.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Customers;

namespace BuildBook.Tests;

public class CustomerReportExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesCustomerWorkbook()
    {
        var reader = new StubCustomerReportReader(
            [
                new CustomerContractReportRow(
                    1,
                    "Acme <Medical>",
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
        var exporter = new CustomerReportExcelExporter(reader);

        var workbook = await exporter.ExportAsync(new CustomerReportFilter
        {
            Scope = CustomerReportScope.CustomersByContractLevel,
            Value = "Gold"
        });

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var workbookXml = ReadEntryText(archive, "xl/workbook.xml");
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");

        Assert.Contains("Customer Reports", workbookXml);
        Assert.Contains("Customer", worksheetXml);
        Assert.Contains("Acme &lt;Medical&gt;", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
    }

    [Fact]
    public async Task ExportAsync_WritesRmaWorkbook()
    {
        var reader = new StubCustomerReportReader(
            [],
            [
                new CustomerSupportRmaReportRow(
                    10,
                    "RMA-0010",
                    RmaStatus.ReadyToShip,
                    "North & South",
                    "Gold",
                    "Active",
                    RmaPriority.Medium,
                    RmaPriority.High,
                    "Device <Test>",
                    "SN-1000",
                    "Boot fault",
                    "5678",
                    new DateOnly(2026, 6, 30),
                    new DateTimeOffset(2026, 6, 30, 12, 0, 0, TimeSpan.Zero),
                    true,
                    false,
                    true)
            ]);
        var exporter = new CustomerReportExcelExporter(reader);

        var workbook = await exporter.ExportAsync(new CustomerReportFilter
        {
            Scope = CustomerReportScope.PriorityMismatch
        });

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");

        Assert.Contains("RMA number", worksheetXml);
        Assert.Contains("Device &lt;Test&gt;", worksheetXml);
        Assert.Contains("North &amp; South", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        using var stream = archive.GetEntry(entryPath)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private sealed class StubCustomerReportReader(
        IReadOnlyList<CustomerContractReportRow> customerRows,
        IReadOnlyList<CustomerSupportRmaReportRow> rmaRows) : ICustomerReportReader
    {
        public Task<IReadOnlyList<CustomerContractReportRow>> ListCustomersAsync(CustomerReportFilter? filter = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(customerRows);
        }

        public Task<IReadOnlyList<CustomerSupportRmaReportRow>> ListRmasAsync(CustomerReportFilter? filter = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(rmaRows);
        }
    }
}
