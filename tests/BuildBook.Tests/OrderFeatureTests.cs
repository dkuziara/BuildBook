using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using BuildBook.Infrastructure.Persistence.Orders;
using System.Reflection;

namespace BuildBook.Tests;

public class OrderFeatureTests
{
    [Fact]
    public void CreateOrderValidator_RequiresTitleStatusAndPriority()
    {
        var errors = CreateOrderValidator.Validate(new CreateOrderRequest
        {
            OrderTitle = " ",
            Status = " ",
            Priority = null
        });

        Assert.Contains("Order title is required.", errors);
        Assert.Contains("Status is required.", errors);
        Assert.Contains("Priority is required.", errors);
    }

    [Fact]
    public void CreateOrderValidator_RejectsDueDateBeforeStartDate()
    {
        var errors = CreateOrderValidator.Validate(new CreateOrderRequest
        {
            OrderTitle = "Demo order",
            Status = BuildBookOrderStatuses.OrderReceived,
            Priority = OrderPriority.Medium,
            StartDate = new DateOnly(2026, 7, 10),
            DueDate = new DateOnly(2026, 7, 9)
        });

        Assert.Contains("Due date cannot be before the start date.", errors);
    }

    [Fact]
    public void OrderRegisterFilter_HasAnyFilterRecognizesActiveValues()
    {
        var filter = new OrderRegisterFilter
        {
            Search = "demo",
            HasLinkedBuildRecord = false
        };

        Assert.True(filter.HasAnyFilter());
    }

    [Fact]
    public void OrderRegisterFilter_HasAnyFilterReturnsFalseWhenEmpty()
    {
        var filter = new OrderRegisterFilter();

        Assert.False(filter.HasAnyFilter());
    }

    [Fact]
    public void OrderPlannerImportSummary_TruncatesWithinRequestedMaximumLength()
    {
        var summarizeMethod = typeof(OrderPlannerImportService).GetMethod(
            "SummarizeText",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(summarizeMethod);

        var summary = (string?)summarizeMethod!.Invoke(null, ["X".PadLeft(1100, 'X'), 1024]);

        Assert.NotNull(summary);
        Assert.True(summary!.Length <= 1024);
        Assert.EndsWith("...", summary, StringComparison.Ordinal);
    }

    [Fact]
    public void OrderRecordCreatorSummary_TruncatesWithinColumnMaximumLength()
    {
        var summarizeMethod = typeof(OrderRecordCreator).GetMethod(
            "SummarizeText",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(summarizeMethod);

        var summary = (string?)summarizeMethod!.Invoke(null, ["Y".PadLeft(1100, 'Y')]);

        Assert.NotNull(summary);
        Assert.True(summary!.Length <= 1024);
        Assert.EndsWith("...", summary, StringComparison.Ordinal);
    }
}
