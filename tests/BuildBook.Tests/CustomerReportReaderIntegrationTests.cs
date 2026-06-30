using BuildBook.Application.Customers;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Customers;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class CustomerReportReaderIntegrationTests
{
    [Fact]
    public async Task ListAsync_FiltersCustomerAndRmaReportsForContractAndDataQualityScenarios()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookCustomerReports");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var goldLevel = new SupportContractLevel
                {
                    Name = "Gold",
                    DefaultRmaPriority = RmaPriority.High,
                    DisplayOrder = 1,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var silverLevel = new SupportContractLevel
                {
                    Name = "Silver",
                    DefaultRmaPriority = RmaPriority.Medium,
                    DisplayOrder = 2,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var goldCustomer = new Customer
                {
                    Name = "Sellafield",
                    SupportContractLevel = goldLevel,
                    SupportContractStatus = CustomerSupportContractStatuses.Active,
                    SupportContractEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var noContractCustomer = new Customer
                {
                    Name = "North Lab",
                    SupportContractStatus = CustomerSupportContractStatuses.NoContract,
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                var expiredCustomer = new Customer
                {
                    Name = "Legacy Sites",
                    SupportContractLevel = silverLevel,
                    SupportContractStatus = CustomerSupportContractStatuses.Expired,
                    SupportContractEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                    CreatedBy = "tester",
                    LastUpdatedBy = "tester",
                    IsActive = true
                };

                setupContext.Customers.AddRange(goldCustomer, noContractCustomer, expiredCustomer);
                await setupContext.SaveChangesAsync();

                setupContext.RmaRecords.AddRange(
                    CreateRma("RMA-0001", goldCustomer, RmaStatus.WorkInProgress, RmaPriority.Medium, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
                    CreateRma("RMA-0002", goldCustomer, RmaStatus.WorkInProgress, RmaPriority.High, "5678", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))),
                    CreateRma("RMA-0003", expiredCustomer, RmaStatus.OnHold, RmaPriority.Low, "7890", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3))),
                    CreateRma("RMA-0004", noContractCustomer, RmaStatus.Closed, null, null, null));

                await setupContext.SaveChangesAsync();
            }

            var reader = new CustomerReportReader(new TestDbContextFactory(options));

            var expiringCustomers = await reader.ListCustomersAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.ContractsExpiringWithinDays,
                Value = "30"
            });
            var noContractCustomers = await reader.ListCustomersAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.CustomersWithNoContract
            });
            var expiredCustomers = await reader.ListCustomersAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.ExpiredContracts
            });
            var openGoldRmas = await reader.ListRmasAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.OpenRmasByContractLevel,
                Value = "Gold"
            });
            var overdueGoldRmas = await reader.ListRmasAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.OverdueRmasByContractLevel,
                Value = "Gold"
            });
            var missingSupportTickets = await reader.ListRmasAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.MissingSupportTicketNumber
            });
            var priorityMismatch = await reader.ListRmasAsync(new CustomerReportFilter
            {
                Scope = CustomerReportScope.PriorityMismatch
            });

            Assert.Single(expiringCustomers);
            Assert.Equal("Sellafield", expiringCustomers[0].CustomerName);
            Assert.Single(noContractCustomers);
            Assert.Equal("North Lab", noContractCustomers[0].CustomerName);
            Assert.Single(expiredCustomers);
            Assert.Equal("Legacy Sites", expiredCustomers[0].CustomerName);

            Assert.Equal(2, openGoldRmas.Count);
            Assert.Single(overdueGoldRmas);
            Assert.Equal("RMA-0001", overdueGoldRmas[0].RmaNumber);

            Assert.Equal(2, missingSupportTickets.Count);
            Assert.Contains(missingSupportTickets, row => row.RmaNumber == "RMA-0001");
            Assert.Contains(missingSupportTickets, row => row.RmaNumber == "RMA-0004");

            Assert.Single(priorityMismatch);
            Assert.Equal("RMA-0001", priorityMismatch[0].RmaNumber);
            Assert.Equal(RmaPriority.High, priorityMismatch[0].SuggestedPriority);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    private static RmaRecord CreateRma(
        string rmaNumber,
        Customer customer,
        RmaStatus status,
        RmaPriority? priority,
        string? supportTicketNumber,
        DateOnly? dueDate)
    {
        return new RmaRecord
        {
            RmaNumber = rmaNumber,
            Customer = customer,
            ProductName = "Device",
            SerialNumber = $"{rmaNumber}-SN",
            FaultSummary = $"Fault for {rmaNumber}",
            InitialFaultDescription = $"Initial fault for {rmaNumber}",
            Status = status,
            Priority = priority,
            SupportTicketNumber = supportTicketNumber,
            DueDate = dueDate,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }
}
