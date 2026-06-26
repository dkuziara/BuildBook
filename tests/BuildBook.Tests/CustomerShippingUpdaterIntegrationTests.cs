using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class CustomerShippingUpdaterIntegrationTests
{
    [Fact]
    public async Task UpdateAsync_PersistsCustomerShippingChangesAndAuditEntries()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCustomerShippingIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int buildRecordId;
            int customerId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = new Customer { Name = "APVL", IsActive = true };
                var recordToUpdate = new BuildRecord
                {
                    ProductCode = "CDM61100",
                    ProductName = "Shipping Device",
                    SerialNumber = "1000000",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                };

                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.Add(recordToUpdate);
                await setupContext.SaveChangesAsync();
                buildRecordId = recordToUpdate.Id;
                customerId = customer.Id;
            }

            var updater = new CustomerShippingUpdater(
                new TestDbContextFactory(options),
                new BuildRecordAuditService());

            var result = await updater.UpdateAsync(
                buildRecordId,
                new UpdateCustomerShippingRequest
                {
                    CustomerId = customerId,
                    CustomerOrder = " PO-100 ",
                    OANumber = " OA-100 ",
                    InvoiceNumber = " INV-100 ",
                    DateShipped = new DateOnly(2026, 6, 26)
                },
                " DOMAIN\\shipping ");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var buildRecord = await verifyContext.BuildRecords.Include(record => record.Customer).SingleAsync();
            var auditEntries = await verifyContext.BuildRecordAudit.OrderBy(entry => entry.FieldChanged).ToListAsync();

            Assert.Equal(customerId, buildRecord.CustomerId);
            Assert.Equal("APVL", buildRecord.Customer?.Name);
            Assert.Equal("PO-100", buildRecord.CustomerOrder);
            Assert.Equal("OA-100", buildRecord.OANumber);
            Assert.Equal("INV-100", buildRecord.InvoiceNumber);
            Assert.Equal(new DateOnly(2026, 6, 26), buildRecord.DateShipped);
            Assert.Equal("DOMAIN\\shipping", buildRecord.LastUpdatedBy);
            Assert.Equal(5, auditEntries.Count);
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "Customer" && entry.NewValue == "APVL");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "DateShipped" && entry.NewValue == "2026-06-26");
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailureWhenSelectedCustomerDoesNotExist()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCustomerShippingIntegration");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int buildRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var recordToUpdate = new BuildRecord
                {
                    ProductCode = "CDM61100",
                    ProductName = "Shipping Device",
                    SerialNumber = "1000000",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester"
                };
                setupContext.BuildRecords.Add(recordToUpdate);
                await setupContext.SaveChangesAsync();
                buildRecordId = recordToUpdate.Id;
            }

            var updater = new CustomerShippingUpdater(
                new TestDbContextFactory(options),
                new BuildRecordAuditService());

            var result = await updater.UpdateAsync(
                buildRecordId,
                new UpdateCustomerShippingRequest
                {
                    CustomerId = 999
                },
                "editor");

            Assert.False(result.Succeeded);
            Assert.Equal(["Selected customer was not found."], result.Errors);

            await using var verifyContext = new BuildBookDbContext(options);
            var buildRecord = await verifyContext.BuildRecords.SingleAsync(record => record.Id == buildRecordId);

            Assert.Null(buildRecord.CustomerId);
            Assert.Empty(await verifyContext.BuildRecordAudit.ToListAsync());
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }
}
