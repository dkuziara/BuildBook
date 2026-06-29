using BuildBook.Application.Rmas;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class RmaRecordServiceIntegrationTests
{
    [Fact]
    public async Task GenerateNextAsync_ReturnsNextSequentialRmaNumber()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaNumberGenerator");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                setupContext.RmaRecords.AddRange(
                    CreateRmaRecord("RMA-0007", "Device A", "SN-0007"),
                    CreateRmaRecord("Legacy-0012", "Device B", "SN-0012"));
                await setupContext.SaveChangesAsync();
            }

            var generator = new RmaNumberGenerator(new TestDbContextFactory(options));

            var nextRmaNumber = await generator.GenerateNextAsync();

            Assert.Equal("RMA-0013", nextRmaNumber);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task CreateAsync_PersistsLinkedRmaChecklistStatusHistoryAndAudit()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaCreate");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            var linkedBuildRecordId = await SeedBuildRecordAsync(
                options,
                "Acme Medical",
                "CDM61100",
                "RadSight Access Terminal",
                "SN-1000");

            var service = CreateService(options);

            var result = await service.CreateAsync(
                new CreateRmaRequest
                {
                    CustomerName = "  Acme Medical  ",
                    ProductName = "  RadSight Access Terminal  ",
                    ProductCode = "  CDM61100  ",
                    SerialNumber = "  SN-1000  ",
                    FaultSummary = "  Does not boot  ",
                    InitialFaultDescription = "  Device powers on and hangs at splash screen.  ",
                    ContactName = "  Alex Repair  ",
                    ContactEmail = "  alex@example.com  ",
                    ContactPhone = "  01234 567890  ",
                    SupportTicketNumber = "  TCK-100  ",
                    SupportTicketUrl = "  https://tickets.example.com/TCK-100  ",
                    OriginalOrderNumber = "  ORD-200  ",
                    OriginalInvoiceNumber = "  INV-300  ",
                    LinkedBuildRecordId = linkedBuildRecordId
                },
                " DOMAIN\\alice ");

            Assert.True(result.Succeeded);
            Assert.NotNull(result.RmaRecordId);

            await using var verifyContext = new BuildBookDbContext(options);
            var rmaRecord = await verifyContext.RmaRecords
                .Include(record => record.Customer)
                .SingleAsync(record => record.Id == result.RmaRecordId);
            var checklistItems = await verifyContext.RmaChecklistItems
                .Where(item => item.RmaRecordId == rmaRecord.Id)
                .OrderBy(item => item.DisplayOrder)
                .ToListAsync();
            var statusHistory = await verifyContext.RmaStatusHistory
                .SingleAsync(entry => entry.RmaRecordId == rmaRecord.Id);
            var auditEntry = await verifyContext.RmaAudit
                .SingleAsync(entry => entry.RmaRecordId == rmaRecord.Id);

            Assert.Equal("RMA-0001", rmaRecord.RmaNumber);
            Assert.Equal(RmaStatus.BookedIn, rmaRecord.Status);
            Assert.Equal(linkedBuildRecordId, rmaRecord.BuildRecordId);
            Assert.Equal("Acme Medical", rmaRecord.Customer?.Name);
            Assert.Equal("RadSight Access Terminal", rmaRecord.ProductName);
            Assert.Equal("CDM61100", rmaRecord.ProductCode);
            Assert.Equal("SN-1000", rmaRecord.SerialNumber);
            Assert.Equal("Does not boot", rmaRecord.FaultSummary);
            Assert.Equal("Device powers on and hangs at splash screen.", rmaRecord.InitialFaultDescription);
            Assert.Equal("DOMAIN\\alice", rmaRecord.CreatedBy);
            Assert.Equal(RmaChecklistTemplate.DefaultItems.Length, checklistItems.Count);
            Assert.Equal(RmaChecklistTemplate.DefaultItems[0], checklistItems[0].Text);
            Assert.Equal(RmaStatus.BookedIn, statusHistory.NewStatus);
            Assert.Equal("RMA created.", statusHistory.Reason);
            Assert.Equal("Created", auditEntry.Action);
            Assert.Equal("DOMAIN\\alice", auditEntry.User);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SearchAsync_FiltersByStatusCustomerAndLinkedBuildRecord()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaSearch");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var acme = CreateCustomer("Acme Medical");
                var beta = CreateCustomer("Beta Clinic");
                var buildRecord = CreateBuildRecord("CDM61100", "RadSight Access Terminal", "SN-1000", acme);

                setupContext.Customers.AddRange(acme, beta);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.AddRange(
                    CreateRmaRecord("RMA-0001", "RadSight Access Terminal", "SN-1000", acme, buildRecord, RmaStatus.BookedIn),
                    CreateRmaRecord("RMA-0002", "RadSight Access Terminal", "SN-1001", acme, null, RmaStatus.WorkInProgress),
                    CreateRmaRecord("RMA-0003", "Other Device", "SN-2000", beta, null, RmaStatus.BookedIn));

                await setupContext.SaveChangesAsync();
            }

            var service = CreateService(options);

            var rows = await service.SearchAsync(new RmaRegisterFilter
            {
                Status = RmaStatus.BookedIn,
                Customer = "Acme",
                HasLinkedBuildRecord = true
            });

            var row = Assert.Single(rows);
            Assert.Equal("RMA-0001", row.RmaNumber);
            Assert.True(row.HasLinkedBuildRecord);
            Assert.Equal("Acme Medical", row.CustomerName);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SuggestBuildRecordMatchesAsync_ReturnsExactSerialMatchWithReasons()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaMatches");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.AddRange(
                    CreateBuildRecord("CDM61100", "RadSight Access Terminal", "SN-1000", customer),
                    CreateBuildRecord("CDM62200", "Other Device", "SN-9999", customer));
                await setupContext.SaveChangesAsync();
            }

            var service = CreateService(options);

            var matches = await service.SuggestBuildRecordMatchesAsync(
                new RmaBuildRecordMatchRequest("SN-1000", "CDM61100", "RadSight Access Terminal", "Acme Medical"));

            var match = Assert.Single(matches);
            Assert.Equal("SN-1000", match.SerialNumber);
            Assert.Contains("Serial number match", match.MatchReasons);
            Assert.Contains("Product code match", match.MatchReasons);
            Assert.Contains("Product name match", match.MatchReasons);
            Assert.Contains("Customer match", match.MatchReasons);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateIntakeAsync_PersistsChangesAndCreatesAuditEntries()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaUpdateIntake");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var existingCustomer = CreateCustomer("Old Customer");
                setupContext.Customers.Add(existingCustomer);

                var rmaRecord = CreateRmaRecord("RMA-0001", "Old Device", "OLD-001", existingCustomer);
                rmaRecord.ProductCode = "OLD-CODE";
                rmaRecord.FaultSummary = "Old fault";
                rmaRecord.InitialFaultDescription = "Old description";
                rmaRecord.ContactName = "Old Contact";
                rmaRecord.SupportTicketNumber = "OLD-TICKET";

                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options);

            var result = await service.UpdateIntakeAsync(
                rmaRecordId,
                new UpdateRmaIntakeRequest
                {
                    CustomerName = "New Customer",
                    ProductName = "Updated Device",
                    ProductCode = "NEW-CODE",
                    SerialNumber = "NEW-001",
                    FaultSummary = "Updated fault",
                    InitialFaultDescription = "Updated initial description",
                    FaultDescription = "Engineer notes added",
                    ContactName = "New Contact",
                    ContactEmail = "new@example.com",
                    ContactPhone = "01234 000000",
                    CustomerAddress = "1 Service Road",
                    CustomerReference = "CUST-REF",
                    SupportTicketNumber = "NEW-TICKET",
                    SupportTicketUrl = "https://tickets.example.com/NEW-TICKET",
                    OriginalOrderNumber = "ORD-500",
                    OriginalOrderDate = new DateOnly(2026, 6, 20),
                    OriginalInvoiceNumber = "INV-600"
                },
                "DOMAIN\\editor");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedRmaRecord = await verifyContext.RmaRecords
                .Include(record => record.Customer)
                .SingleAsync(record => record.Id == rmaRecordId);
            var auditEntries = await verifyContext.RmaAudit
                .Where(entry => entry.RmaRecordId == rmaRecordId)
                .ToListAsync();

            Assert.Equal("New Customer", savedRmaRecord.Customer?.Name);
            Assert.Equal("Updated Device", savedRmaRecord.ProductName);
            Assert.Equal("NEW-CODE", savedRmaRecord.ProductCode);
            Assert.Equal("NEW-001", savedRmaRecord.SerialNumber);
            Assert.Equal("Updated fault", savedRmaRecord.FaultSummary);
            Assert.Equal("Updated initial description", savedRmaRecord.InitialFaultDescription);
            Assert.Equal("Engineer notes added", savedRmaRecord.FaultDescription);
            Assert.Equal("DOMAIN\\editor", savedRmaRecord.LastUpdatedBy);
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "Customer" && entry.NewValue == "New Customer");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "SupportTicketNumber" && entry.NewValue == "NEW-TICKET");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "OriginalOrderDate" && entry.NewValue == "2026-06-20");
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task LinkBuildRecordAsync_AndUnlinkBuildRecordAsync_UpdateRelationAndAudit()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaLinking");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;
            int buildRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                var buildRecord = CreateBuildRecord("CDM61100", "RadSight Access Terminal", "SN-1000", customer);
                var rmaRecord = CreateRmaRecord("RMA-0001", "RadSight Access Terminal", "SN-1000", customer);

                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();

                rmaRecordId = rmaRecord.Id;
                buildRecordId = buildRecord.Id;
            }

            var service = CreateService(options);

            var linkResult = await service.LinkBuildRecordAsync(rmaRecordId, buildRecordId, "DOMAIN\\editor");
            var unlinkResult = await service.UnlinkBuildRecordAsync(rmaRecordId, "DOMAIN\\editor");

            Assert.True(linkResult.Succeeded);
            Assert.True(unlinkResult.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedRmaRecord = await verifyContext.RmaRecords.SingleAsync(record => record.Id == rmaRecordId);
            var auditEntries = await verifyContext.RmaAudit
                .Where(entry => entry.RmaRecordId == rmaRecordId && entry.FieldChanged == "BuildRecordId")
                .OrderBy(entry => entry.Id)
                .ToListAsync();

            Assert.Null(savedRmaRecord.BuildRecordId);
            Assert.Equal(2, auditEntries.Count);
            Assert.Equal(buildRecordId.ToString(), auditEntries[0].NewValue);
            Assert.Null(auditEntries[1].NewValue);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    private static RmaRecordService CreateService(DbContextOptions<BuildBookDbContext> options)
    {
        return new RmaRecordService(
            new TestDbContextFactory(options),
            new RmaAuditService());
    }

    private static async Task<int> SeedBuildRecordAsync(
        DbContextOptions<BuildBookDbContext> options,
        string customerName,
        string productCode,
        string productName,
        string serialNumber)
    {
        await using var context = new BuildBookDbContext(options);
        var customer = CreateCustomer(customerName);
        var buildRecord = CreateBuildRecord(productCode, productName, serialNumber, customer);

        context.Customers.Add(customer);
        context.BuildRecords.Add(buildRecord);
        await context.SaveChangesAsync();

        return buildRecord.Id;
    }

    private static Customer CreateCustomer(string name)
    {
        return new Customer
        {
            Name = name,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }

    private static BuildRecord CreateBuildRecord(string productCode, string productName, string serialNumber, Customer customer)
    {
        return new BuildRecord
        {
            ProductCode = productCode,
            ProductName = productName,
            SerialNumber = serialNumber,
            Customer = customer,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }

    private static RmaRecord CreateRmaRecord(
        string rmaNumber,
        string productName,
        string serialNumber,
        Customer? customer = null,
        BuildRecord? buildRecord = null,
        RmaStatus status = RmaStatus.BookedIn)
    {
        return new RmaRecord
        {
            RmaNumber = rmaNumber,
            Customer = customer,
            ProductName = productName,
            SerialNumber = serialNumber,
            FaultSummary = $"Fault for {serialNumber}",
            InitialFaultDescription = $"Initial fault for {serialNumber}",
            BuildRecord = buildRecord,
            Status = status,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }
}
