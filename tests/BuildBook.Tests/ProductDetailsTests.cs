using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;

namespace BuildBook.Tests;

public class ProductDetailsTests
{
    [Fact]
    public void ValidatorAcceptsProductDetailsFields()
    {
        var request = new UpdateProductDetailsRequest
        {
            ProductCode = "CDM61100",
            ProductName = "RadSight Access Terminal",
            ProductClassification = "Terminal",
            SerialNumber = "1000000",
            InternalStatus = InternalStatus.Checked
        };

        var errors = UpdateProductDetailsValidator.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatorRequiresProductCodeProductNameAndSerialNumber()
    {
        var request = new UpdateProductDetailsRequest();

        var errors = UpdateProductDetailsValidator.Validate(request);

        Assert.Equal(
            [
                "Product code is required.",
                "Product name is required.",
                "Serial number is required."
            ],
            errors);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateProductDetailsResult.Failure("Serial number is required.");

        Assert.False(result.Succeeded);
        Assert.Equal(["Serial number is required."], result.Errors);
    }
}
