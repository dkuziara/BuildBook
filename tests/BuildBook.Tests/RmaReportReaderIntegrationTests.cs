using BuildBook.Application.Rmas;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class RmaReportReaderIntegrationTests
{
    [Fact]
    public async Task ListAsync_ComputesFiltersRepeatReturnsAndCommercialStates()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaReports");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                var buildRecord = CreateBuildRecord("CDM61100", "Device A", "SN-1000", customer);
                var workInProgressRma = CreateRmaRecord("RMA-0001", "Device A", "SN-1000", customer, buildRecord, RmaStatus.WorkInProgress);
                var shippedRma = CreateRmaRecord("RMA-0002", "Device A", "SN-1000", customer, buildRecord, RmaStatus.Shipped);

                workInProgressRma.CreatedAt = new DateTimeOffset(2026, 6, 10, 8, 0, 0, TimeSpan.Zero);
                workInProgressRma.LastUpdatedAt = new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);
                workInProgressRma.FaultCategory = RmaFaultCategory.HardwareFailure;
                workInProgressRma.RootCauseCategory = RmaRootCauseCategory.ComponentFailure;
                workInProgressRma.WarrantyStatus = RmaWarrantyStatus.OutOfWarranty;
                workInProgressRma.ChargeableRepair = true;
                workInProgressRma.CustomerApprovalRequired = true;
                workInProgressRma.CustomerApprovalReceived = false;
                workInProgressRma.OnHoldReason = null;
                workInProgressRma.DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
                workInProgressRma.DateItemReceived = new DateOnly(2026, 6, 11);

                shippedRma.CreatedAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
                shippedRma.LastUpdatedAt = new DateTimeOffset(2026, 6, 28, 16, 0, 0, TimeSpan.Zero);
                shippedRma.ChargeableRepair = true;
                shippedRma.CustomerApprovalRequired = true;
                shippedRma.CustomerApprovalReceived = true;
                shippedRma.ShippedDate = new DateOnly(2026, 6, 28);
                shippedRma.DateItemReceived = new DateOnly(2026, 6, 2);
                shippedRma.RepairCompletedDate = new DateOnly(2026, 6, 25);

                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.AddRange(workInProgressRma, shippedRma);
                await setupContext.SaveChangesAsync();

                setupContext.RmaStatusHistory.AddRange(
                    new RmaStatusHistory
                    {
                        RmaRecordId = workInProgressRma.Id,
                        OldStatus = null,
                        NewStatus = RmaStatus.BookedIn,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 10, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = workInProgressRma.Id,
                        OldStatus = RmaStatus.BookedIn,
                        NewStatus = RmaStatus.WorkInProgress,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 11, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = workInProgressRma.Id,
                        OldStatus = RmaStatus.WorkInProgress,
                        NewStatus = RmaStatus.OnHold,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = workInProgressRma.Id,
                        OldStatus = RmaStatus.OnHold,
                        NewStatus = RmaStatus.WorkInProgress,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 18, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = shippedRma.Id,
                        OldStatus = null,
                        NewStatus = RmaStatus.BookedIn,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = shippedRma.Id,
                        OldStatus = RmaStatus.BookedIn,
                        NewStatus = RmaStatus.WorkInProgress,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 2, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = shippedRma.Id,
                        OldStatus = RmaStatus.WorkInProgress,
                        NewStatus = RmaStatus.ReadyToShip,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 26, 8, 0, 0, TimeSpan.Zero)
                    },
                    new RmaStatusHistory
                    {
                        RmaRecordId = shippedRma.Id,
                        OldStatus = RmaStatus.ReadyToShip,
                        NewStatus = RmaStatus.Shipped,
                        ChangedBy = "tester",
                        ChangedAt = new DateTimeOffset(2026, 6, 28, 8, 0, 0, TimeSpan.Zero)
                    });

                await setupContext.SaveChangesAsync();
            }

            var reader = new RmaReportReader(new TestDbContextFactory(options));

            var allRows = await reader.ListAsync();
            var firstRow = Assert.Single(allRows, row => row.RmaNumber == "RMA-0001");
            var secondRow = Assert.Single(allRows, row => row.RmaNumber == "RMA-0002");
            var overdueRows = await reader.ListAsync(new RmaReportFilter { Scope = RmaReportScope.OperationalOverdue });
            var awaitingApprovalRows = await reader.ListAsync(new RmaReportFilter { Scope = RmaReportScope.AwaitingApproval });
            var awaitingPaymentRows = await reader.ListAsync(new RmaReportFilter { Scope = RmaReportScope.AwaitingPayment });
            var repeatReturnRows = await reader.ListAsync(new RmaReportFilter { Scope = RmaReportScope.RepeatReturns });
            var metrics = await reader.GetMetricsAsync(firstRow.Id);

            Assert.Equal(2, allRows.Count);
            Assert.Equal(1, firstRow.PreviousRmaCount);
            Assert.Equal(1, secondRow.PreviousRmaCount);
            Assert.True(firstRow.DaysOnHold >= 3);
            Assert.Equal(23, secondRow.RepairDays);
            Assert.Equal(2, secondRow.ReadyToShipToShippedDays);
            Assert.Single(overdueRows);
            Assert.Equal("RMA-0001", overdueRows[0].RmaNumber);
            Assert.Single(awaitingApprovalRows);
            Assert.Equal("RMA-0001", awaitingApprovalRows[0].RmaNumber);
            Assert.Single(awaitingPaymentRows);
            Assert.Equal("RMA-0002", awaitingPaymentRows[0].RmaNumber);
            Assert.Equal(2, repeatReturnRows.Count);
            Assert.NotNull(metrics);
            Assert.True(metrics!.DaysOpen > 0);
            Assert.True(metrics.DaysInCurrentStatus > 0);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
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
