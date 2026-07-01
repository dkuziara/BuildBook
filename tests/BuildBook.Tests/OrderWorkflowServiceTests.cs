using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using BuildBook.Infrastructure.Persistence.Orders;
using System.Reflection;

namespace BuildBook.Tests;

public class OrderWorkflowServiceTests
{
    [Fact]
    public async Task UpdateWorkflowAsync_RejectsDueDateBeforeStartDate()
    {
        var service = new OrderWorkflowService(new TestDbContextFactory(DatabaseTestHelper.CreateSqlServerOptions(nameof(UpdateWorkflowAsync_RejectsDueDateBeforeStartDate))));

        var result = await service.UpdateWorkflowAsync(
            123,
            new UpdateOrderWorkflowRequest
            {
                StartDate = new DateOnly(2026, 7, 10),
                DueDate = new DateOnly(2026, 7, 9)
            },
            "DOMAIN\\planner");

        Assert.False(result.Succeeded);
        Assert.Contains("Due date cannot be before the start date.", result.Errors);
    }

    [Fact]
    public async Task SaveAssignmentAsync_RequiresBuildBookUserOrImportedUserText()
    {
        var service = new OrderWorkflowService(new TestDbContextFactory(DatabaseTestHelper.CreateSqlServerOptions(nameof(SaveAssignmentAsync_RequiresBuildBookUserOrImportedUserText))));

        var result = await service.SaveAssignmentAsync(
            123,
            new SaveOrderAssignmentRequest
            {
                AssignmentType = OrderAssignmentType.Owner
            },
            "DOMAIN\\planner");

        Assert.False(result.Succeeded);
        Assert.Contains("Select a BuildBook user or enter an imported user name.", result.Errors);
    }

    [Fact]
    public async Task ChangeStatusAsync_RejectsUnknownStatuses()
    {
        var service = new OrderWorkflowService(new TestDbContextFactory(DatabaseTestHelper.CreateSqlServerOptions(nameof(ChangeStatusAsync_RejectsUnknownStatuses))));

        var result = await service.ChangeStatusAsync(
            123,
            new ChangeOrderStatusRequest
            {
                NewStatus = "Unknown Status"
            },
            "DOMAIN\\planner");

        Assert.False(result.Succeeded);
        Assert.Contains("The selected status is not valid.", result.Errors);
    }

    [Fact]
    public void BuildStatusWarnings_ReturnsChecklistAndAssignmentWarningsForLaterWorkflowStages()
    {
        var buildWarningsMethod = typeof(OrderWorkflowService).GetMethod(
            "BuildStatusWarnings",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildWarningsMethod);

        var orderRecord = new OrderRecord
        {
            OrderNumber = "ORD-2001",
            OrderTitle = "Warning order",
            Status = BuildBookOrderStatuses.OrderReceived
        };

        orderRecord.ChecklistItems.Add(new OrderChecklistItem
        {
            DisplayOrder = 1,
            Text = "Pack hardware",
            IsCompleted = false
        });

        var warnings = (List<string>?)buildWarningsMethod!.Invoke(null, [orderRecord, BuildBookOrderStatuses.PreparedForShipping]);

        Assert.NotNull(warnings);
        Assert.Contains("This Order has no assignments yet.", warnings!);
        Assert.Contains("Checklist items are still incomplete.", warnings);
    }

    [Fact]
    public void BuildStatusWarnings_ReturnsShippingWarningForShippedStatusWithoutDate()
    {
        var buildWarningsMethod = typeof(OrderWorkflowService).GetMethod(
            "BuildStatusWarnings",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildWarningsMethod);

        var orderRecord = new OrderRecord
        {
            OrderNumber = "ORD-2002",
            OrderTitle = "Shipping warning order",
            Status = BuildBookOrderStatuses.PreparedForShipping,
            ShippedDate = null
        };

        var warnings = (List<string>?)buildWarningsMethod!.Invoke(null, [orderRecord, BuildBookOrderStatuses.Shipped]);

        Assert.NotNull(warnings);
        Assert.Contains("Status is Shipped but shipped date is missing.", warnings!);
    }

    [Fact]
    public void BuildStatusWarnings_ReturnsInvoicingReadinessWarningsWhenContractReadyStatusIsMissingFields()
    {
        var buildWarningsMethod = typeof(OrderWorkflowService).GetMethod(
            "BuildStatusWarnings",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildWarningsMethod);

        var orderRecord = new OrderRecord
        {
            OrderNumber = "ORD-2003",
            OrderTitle = "Invoicing readiness warning order",
            Status = BuildBookOrderStatuses.Shipped,
            ContractReadyForInvoicing = false,
            ReadyForInvoicingDate = null
        };

        var warnings = (List<string>?)buildWarningsMethod!.Invoke(null, [orderRecord, BuildBookOrderStatuses.ContractReadyForInvoicing]);

        Assert.NotNull(warnings);
        Assert.Contains("Status is Contract Ready for Invoicing but invoice readiness date is missing.", warnings!);
        Assert.Contains("Contract ready for invoicing has not been marked.", warnings);
    }

    [Fact]
    public void BuildStatusWarnings_ReturnsInvoiceWarningsForInvoicedStatusWithoutNumberOrDate()
    {
        var buildWarningsMethod = typeof(OrderWorkflowService).GetMethod(
            "BuildStatusWarnings",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildWarningsMethod);

        var orderRecord = new OrderRecord
        {
            OrderNumber = "ORD-2004",
            OrderTitle = "Invoice warning order",
            Status = BuildBookOrderStatuses.ContractReadyForInvoicing,
            InvoiceNumber = null,
            InvoicedDate = null
        };

        var warnings = (List<string>?)buildWarningsMethod!.Invoke(null, [orderRecord, BuildBookOrderStatuses.Invoiced]);

        Assert.NotNull(warnings);
        Assert.Contains("Status is Invoiced but invoice number is missing.", warnings!);
        Assert.Contains("Status is Invoiced but invoiced date is missing.", warnings);
    }
}
