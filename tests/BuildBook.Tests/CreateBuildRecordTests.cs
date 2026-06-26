using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class CreateBuildRecordTests
{
    [Fact]
    public void ValidatorAcceptsRequiredBuildRecordFields()
    {
        var request = new CreateBuildRecordRequest
        {
            ProductCode = "CDM61100",
            ProductName = "RadSight Access Terminal",
            SerialNumber = "1000000"
        };

        var errors = CreateBuildRecordValidator.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatorRequiresProductCodeProductNameAndSerialNumber()
    {
        var request = new CreateBuildRecordRequest();

        var errors = CreateBuildRecordValidator.Validate(request);

        Assert.Equal(
            [
                "Product code is required.",
                "Product name is required.",
                "Serial number is required."
            ],
            errors);
    }

    [Fact]
    public void ValidatorRejectsWhitespaceOnlyRequiredFields()
    {
        var request = new CreateBuildRecordRequest
        {
            ProductCode = "   ",
            ProductName = "\t",
            SerialNumber = " "
        };

        var errors = CreateBuildRecordValidator.Validate(request);

        Assert.Equal(
            [
                "Product code is required.",
                "Product name is required.",
                "Serial number is required."
            ],
            errors);
    }

    [Fact]
    public void FailureResultDoesNotExposeBuildRecordId()
    {
        var result = CreateBuildRecordResult.Failure("A Build Record with this serial number already exists.");

        Assert.False(result.Succeeded);
        Assert.Null(result.BuildRecordId);
        Assert.Equal(["A Build Record with this serial number already exists."], result.Errors);
    }
}
