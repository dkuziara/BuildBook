using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class BuildRecordDetailReaderIntegrationTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsProjectedActiveBuildRecord()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookDetailReaderIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int buildRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = new Customer { Name = "APVL", IsActive = true };
                var buildRecord = new BuildRecord
                {
                    ProductCode = "CDM61100",
                    ProductName = "Detail Device",
                    ProductClassification = "Terminal",
                    SerialNumber = "1000000",
                    Customer = customer,
                    CustomerOrder = "PO-100",
                    OANumber = "OA-100",
                    InvoiceNumber = "INV-100",
                    MachineName = "RADSIGHT-100",
                    PanelDeviceModel = "Panel 1",
                    RadSightVersion = "1.3.6",
                    WindowsVersion = "Windows 10",
                    DateShipped = new DateOnly(2026, 6, 26),
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                };

                setupContext.BuildRecords.Add(buildRecord);
                await setupContext.SaveChangesAsync();
                buildRecordId = buildRecord.Id;
            }

            var reader = new BuildRecordDetailReader(new TestDbContextFactory(options));

            var detail = await reader.GetByIdAsync(buildRecordId);

            Assert.NotNull(detail);
            Assert.Equal(buildRecordId, detail!.Id);
            Assert.Equal("CDM61100", detail.ProductCode);
            Assert.Equal("Detail Device", detail.ProductName);
            Assert.Equal("APVL", detail.CustomerName);
            Assert.Equal("PO-100", detail.CustomerOrder);
            Assert.Equal("RADSIGHT-100", detail.MachineName);
            Assert.Equal("1.3.6", detail.RadSightVersion);
            Assert.Equal("Windows 10", detail.WindowsVersion);
            Assert.Equal(new DateOnly(2026, 6, 26), detail.DateShipped);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForInactiveBuildRecord()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookDetailReaderIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int buildRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var buildRecord = new BuildRecord
                {
                    ProductCode = "CDM61100",
                    ProductName = "Inactive Device",
                    SerialNumber = "1000000",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = false
                };

                setupContext.BuildRecords.Add(buildRecord);
                await setupContext.SaveChangesAsync();
                buildRecordId = buildRecord.Id;
            }

            var reader = new BuildRecordDetailReader(new TestDbContextFactory(options));

            var detail = await reader.GetByIdAsync(buildRecordId);

            Assert.Null(detail);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }
}
