using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class ProductDetailsUpdaterIntegrationTests
{
    [Fact]
    public async Task UpdateAsync_PersistsTrimmedFieldsAndAuditEntries()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookProductDetailsIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int buildRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var recordToUpdate = new BuildRecord
                {
                    ProductCode = "OLD-100",
                    ProductName = "Old Name",
                    ProductClassification = "Legacy",
                    SerialNumber = "1000000",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                };
                setupContext.BuildRecords.Add(recordToUpdate);
                await setupContext.SaveChangesAsync();
                buildRecordId = recordToUpdate.Id;
            }

            var updater = new ProductDetailsUpdater(
                new TestDbContextFactory(options),
                new BuildRecordAuditService());

            var result = await updater.UpdateAsync(
                buildRecordId,
                new UpdateProductDetailsRequest
                {
                    ProductCode = " NEW-100 ",
                    ProductName = " Updated Device ",
                    ProductClassification = " Terminal ",
                    SerialNumber = " 1000001 ",
                    InternalStatus = InternalStatus.Checked
                },
                " DOMAIN\\editor ");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var buildRecord = await verifyContext.BuildRecords.SingleAsync(record => record.Id == buildRecordId);
            var auditEntries = await verifyContext.BuildRecordAudit
                .Where(entry => entry.BuildRecordId == buildRecordId)
                .OrderBy(entry => entry.FieldChanged)
                .ToListAsync();

            Assert.Equal("NEW-100", buildRecord.ProductCode);
            Assert.Equal("Updated Device", buildRecord.ProductName);
            Assert.Equal("Terminal", buildRecord.ProductClassification);
            Assert.Equal("1000001", buildRecord.SerialNumber);
            Assert.Equal(InternalStatus.Checked, buildRecord.InternalStatus);
            Assert.Equal("DOMAIN\\editor", buildRecord.LastUpdatedBy);
            Assert.Equal(5, auditEntries.Count);
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "ProductCode" && entry.NewValue == "NEW-100");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "SerialNumber" && entry.NewValue == "1000001");
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailureWhenSerialNumberWouldDuplicateAnotherRecord()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookProductDetailsIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int targetId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var target = new BuildRecord
                {
                    ProductCode = "CDM61100",
                    ProductName = "Target Device",
                    SerialNumber = "1000000",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                };
                var existing = new BuildRecord
                {
                    ProductCode = "CDM61101",
                    ProductName = "Existing Device",
                    SerialNumber = "1000001",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                };

                setupContext.BuildRecords.AddRange(target, existing);
                await setupContext.SaveChangesAsync();
                targetId = target.Id;
            }

            var updater = new ProductDetailsUpdater(
                new TestDbContextFactory(options),
                new BuildRecordAuditService());

            var result = await updater.UpdateAsync(
                targetId,
                new UpdateProductDetailsRequest
                {
                    ProductCode = "CDM61100",
                    ProductName = "Target Device",
                    SerialNumber = "1000001"
                },
                "editor");

            Assert.False(result.Succeeded);
            Assert.Equal(["A Build Record with this serial number already exists."], result.Errors);

            await using var verifyContext = new BuildBookDbContext(options);
            var targetRecord = await verifyContext.BuildRecords.SingleAsync(record => record.Id == targetId);

            Assert.Equal("1000000", targetRecord.SerialNumber);
            Assert.Empty(await verifyContext.BuildRecordAudit.ToListAsync());
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }
}
