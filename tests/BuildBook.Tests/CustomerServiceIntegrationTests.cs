using BuildBook.Application.Customers;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Orders;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Customers;
using BuildBook.Infrastructure.Persistence.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BuildBook.Tests;

public class CustomerServiceIntegrationTests
{
    [Fact]
    public async Task UpdateAsync_WhenSupportContractChanges_BackfillsBlankLinkedRmaFields()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCustomerContractPropagation");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int customerId;
            int goldLevelId;
            int blankRmaId;
            int existingValuesRmaId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var goldLevel = new SupportContractLevel
                {
                    Name = "Gold",
                    Description = "Priority support",
                    TargetResponseTimeValue = 4,
                    TargetResponseTimeUnit = SupportResponseTimeUnit.WorkingHours,
                    DefaultRmaPriority = RmaPriority.High,
                    RmaPriorityWeight = 100,
                    DisplayOrder = 1,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var customer = new Customer
                {
                    Name = "Acme Medical",
                    SupportContractStatus = CustomerSupportContractStatuses.NoContract,
                    SupportContractEndDate = new DateOnly(2027, 6, 30),
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var blankRma = new RmaRecord
                {
                    RmaNumber = "RMA-0001",
                    Customer = customer,
                    ProductName = "Device A",
                    SerialNumber = "SN-1000",
                    FaultSummary = "Fault A",
                    InitialFaultDescription = "Initial fault A",
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var existingValuesRma = new RmaRecord
                {
                    RmaNumber = "RMA-0002",
                    Customer = customer,
                    ProductName = "Device B",
                    SerialNumber = "SN-2000",
                    FaultSummary = "Fault B",
                    InitialFaultDescription = "Initial fault B",
                    Priority = RmaPriority.Medium,
                    WarrantyStatus = RmaWarrantyStatus.InWarranty,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                setupContext.SupportContractLevels.Add(goldLevel);
                setupContext.Customers.Add(customer);
                setupContext.RmaRecords.AddRange(blankRma, existingValuesRma);
                await setupContext.SaveChangesAsync();

                customerId = customer.Id;
                goldLevelId = goldLevel.Id;
                blankRmaId = blankRma.Id;
                existingValuesRmaId = existingValuesRma.Id;
            }

            var service = new CustomerService(
                new TestDbContextFactory(options),
                new RmaAuditService(),
                CreateStorage());

            var result = await service.UpdateAsync(
                customerId,
                new UpdateCustomerRequest
                {
                    Name = "Acme Medical",
                    SupportContractLevelId = goldLevelId,
                    SupportContractStatus = CustomerSupportContractStatuses.Active,
                    IsActive = true
                },
                " DOMAIN\\editor ");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedCustomer = await verifyContext.Customers.SingleAsync(entry => entry.Id == customerId);
            var savedBlankRma = await verifyContext.RmaRecords.SingleAsync(entry => entry.Id == blankRmaId);
            var savedExistingValuesRma = await verifyContext.RmaRecords.SingleAsync(entry => entry.Id == existingValuesRmaId);
            var auditEntries = await verifyContext.RmaAudit
                .Where(entry => entry.RmaRecordId == blankRmaId)
                .OrderBy(entry => entry.FieldChanged)
                .ToListAsync();

            Assert.Equal(goldLevelId, savedCustomer.SupportContractLevelId);
            Assert.Equal(CustomerSupportContractStatuses.Active, savedCustomer.SupportContractStatus);

            Assert.Equal(RmaPriority.High, savedBlankRma.Priority);
            Assert.Equal(RmaWarrantyStatus.ExtendedWarranty, savedBlankRma.WarrantyStatus);
            Assert.Equal(new DateOnly(2027, 6, 30), savedBlankRma.WarrantyExpiryDate);
            Assert.Equal("DOMAIN\\editor", savedBlankRma.LastUpdatedBy);
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "Priority" && entry.NewValue == nameof(RmaPriority.High));
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "WarrantyStatus" && entry.NewValue == nameof(RmaWarrantyStatus.ExtendedWarranty));
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "WarrantyExpiryDate" && entry.NewValue == "2027-06-30");

            Assert.Equal(RmaPriority.Medium, savedExistingValuesRma.Priority);
            Assert.Equal(RmaWarrantyStatus.InWarranty, savedExistingValuesRma.WarrantyStatus);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SaveAndDeleteContractDocumentAsync_PersistsAndRetrievesCustomerFiles()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCustomerContractDocuments");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);
        var documentRoot = Path.Combine(Path.GetTempPath(), "BuildBookTests", Guid.NewGuid().ToString("N"));

        try
        {
            int customerId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = new Customer
                {
                    Name = "Acme Medical",
                    SupportContractStatus = CustomerSupportContractStatuses.Active,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                setupContext.Customers.Add(customer);
                await setupContext.SaveChangesAsync();
                customerId = customer.Id;
            }

            var service = new CustomerService(
                new TestDbContextFactory(options),
                new RmaAuditService(),
                CreateStorage(documentRoot));

            await using var uploadStream = new MemoryStream("contract-pdf"u8.ToArray());
            var saveResult = await service.SaveContractDocumentAsync(
                customerId,
                new SaveCustomerContractDocumentRequest
                {
                    FileName = "support-contract.pdf",
                    ContentType = "application/pdf",
                    DocumentType = "Support contract",
                    Description = "Current active agreement."
                },
                uploadStream,
                "DOMAIN\\contracts");

            Assert.True(saveResult.Succeeded);

            var detail = await service.GetDetailAsync(customerId);
            Assert.NotNull(detail);

            var document = Assert.Single(detail!.ContractDocuments);
            Assert.Equal("support-contract.pdf", document.FileName);
            Assert.Equal("Support contract", document.DocumentType);
            Assert.Equal("Current active agreement.", document.Description);

            var content = await service.GetContractDocumentContentAsync(customerId, document.Id);
            Assert.NotNull(content);
            Assert.Equal("application/pdf", content!.ContentType);
            Assert.Equal("contract-pdf", System.Text.Encoding.UTF8.GetString(content.Content));

            var deleteResult = await service.DeleteContractDocumentAsync(customerId, document.Id, "DOMAIN\\contracts");
            Assert.True(deleteResult.Succeeded);

            var refreshedDetail = await service.GetDetailAsync(customerId);
            Assert.NotNull(refreshedDetail);
            Assert.Empty(refreshedDetail!.ContractDocuments);
        }
        finally
        {
            if (Directory.Exists(documentRoot))
            {
                Directory.Delete(documentRoot, recursive: true);
            }

            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsLinkedOrdersForCustomer()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCustomerLinkedOrders");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int customerId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = new Customer
                {
                    Name = "Acme Medical",
                    SupportContractStatus = CustomerSupportContractStatuses.Active,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var linkedOrder = new OrderRecord
                {
                    Customer = customer,
                    OrderNumber = "ORD-1001",
                    OrderTitle = "Install upgrade kit",
                    Status = "Prepared For Shipping",
                    Priority = OrderPriority.High,
                    DueDate = new DateOnly(2026, 7, 15),
                    SupportTicketNo = "SUP-4455",
                    CreatedByUserId = null,
                    LastUpdatedAt = new DateTimeOffset(2026, 7, 2, 9, 30, 0, TimeSpan.Zero),
                    IsActive = true
                };

                var inactiveOrder = new OrderRecord
                {
                    Customer = customer,
                    OrderNumber = "ORD-1002",
                    OrderTitle = "Inactive order",
                    Status = "Invoiced",
                    LastUpdatedAt = new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.Zero),
                    IsActive = false
                };

                setupContext.Customers.Add(customer);
                setupContext.OrderRecords.AddRange(linkedOrder, inactiveOrder);
                await setupContext.SaveChangesAsync();
                customerId = customer.Id;
            }

            var service = new CustomerService(
                new TestDbContextFactory(options),
                new RmaAuditService(),
                CreateStorage());

            var detail = await service.GetDetailAsync(customerId);

            Assert.NotNull(detail);
            var order = Assert.Single(detail!.LinkedOrders);
            Assert.Equal("ORD-1001", order.OrderNumber);
            Assert.Equal("Install upgrade kit", order.OrderTitle);
            Assert.Equal("Prepared For Shipping", order.Status);
            Assert.Equal(OrderPriority.High, order.Priority);
            Assert.Equal(new DateOnly(2026, 7, 15), order.DueDate);
            Assert.Equal("SUP-4455", order.SupportTicketNumber);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    private static ICustomerContractDocumentStorage CreateStorage(string? documentRoot = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BuildBook:CustomerContractDocumentStorageDirectory"] = documentRoot ?? Path.Combine(Path.GetTempPath(), "BuildBookTests", Guid.NewGuid().ToString("N"))
            })
            .Build();

        return new LocalCustomerContractDocumentStorage(configuration);
    }
}
