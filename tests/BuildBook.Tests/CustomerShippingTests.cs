using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class CustomerShippingTests
{
    [Fact]
    public void RequestStoresCustomerShippingFields()
    {
        var request = new UpdateCustomerShippingRequest
        {
            CustomerId = 12,
            CustomerOrder = "PO-100",
            OANumber = "OA-100",
            InvoiceNumber = "INV-100",
            DateShipped = new DateOnly(2026, 6, 24)
        };

        Assert.Equal(12, request.CustomerId);
        Assert.Equal("PO-100", request.CustomerOrder);
        Assert.Equal("OA-100", request.OANumber);
        Assert.Equal("INV-100", request.InvoiceNumber);
        Assert.Equal(new DateOnly(2026, 6, 24), request.DateShipped);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateCustomerShippingResult.Failure("Selected customer was not found.");

        Assert.False(result.Succeeded);
        Assert.Equal(["Selected customer was not found."], result.Errors);
    }
}
