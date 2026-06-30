using BuildBook.Application.Customers;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Customers;
using BuildBook.Infrastructure.Persistence.Rmas;
using Microsoft.EntityFrameworkCore;

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
                new RmaAuditService());

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
}
