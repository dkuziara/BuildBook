using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class BuildRecordCreatorIntegrationTests
{
    [Fact]
    public async Task CreateAsync_PersistsTrimmedBuildRecordAndAuditEntry()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCreatorIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            var creator = new BuildRecordCreator(
                new TestDbContextFactory(options),
                new BuildRecordAuditService());

            var result = await creator.CreateAsync(
                new CreateBuildRecordRequest
                {
                    ProductCode = " CDM61100 ",
                    ProductName = " RadSight Access Terminal ",
                    SerialNumber = " 1000000 "
                },
                " DOMAIN\\alice ");

            Assert.True(result.Succeeded);
            Assert.NotNull(result.BuildRecordId);

            await using var verifyContext = new BuildBookDbContext(options);
            var buildRecord = await verifyContext.BuildRecords.SingleAsync();
            var auditEntry = await verifyContext.BuildRecordAudit.SingleAsync();

            Assert.Equal("CDM61100", buildRecord.ProductCode);
            Assert.Equal("RadSight Access Terminal", buildRecord.ProductName);
            Assert.Equal("1000000", buildRecord.SerialNumber);
            Assert.Equal("DOMAIN\\alice", buildRecord.CreatedBy);
            Assert.Equal(AuditAction.Created, auditEntry.Action);
            Assert.Equal(buildRecord.Id, auditEntry.BuildRecordId);
            Assert.Equal("DOMAIN\\alice", auditEntry.User);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task CreateAsync_ReturnsFailureWhenSerialNumberAlreadyExists()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCreatorIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                setupContext.BuildRecords.Add(new BuildRecord
                {
                    ProductCode = "CDM61100",
                    ProductName = "Existing Device",
                    SerialNumber = "1000000",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                });
                await setupContext.SaveChangesAsync();
            }

            var creator = new BuildRecordCreator(
                new TestDbContextFactory(options),
                new BuildRecordAuditService());

            var result = await creator.CreateAsync(
                new CreateBuildRecordRequest
                {
                    ProductCode = "CDM61101",
                    ProductName = "New Device",
                    SerialNumber = "1000000"
                },
                "tester");

            Assert.False(result.Succeeded);
            Assert.Equal(["A Build Record with this serial number already exists."], result.Errors);

            await using var verifyContext = new BuildBookDbContext(options);
            Assert.Equal(1, await verifyContext.BuildRecords.CountAsync());
            Assert.Empty(await verifyContext.BuildRecordAudit.ToListAsync());
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }
}
