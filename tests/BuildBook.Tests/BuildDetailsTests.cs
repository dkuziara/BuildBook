using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class BuildDetailsTests
{
    [Fact]
    public void RequestStoresBuildDetailsFields()
    {
        var request = new UpdateBuildDetailsRequest
        {
            AssembledIn = "Production",
            AssembledBy = "Assembly Team",
            DateAssembled = new DateOnly(2026, 6, 24),
            HardwareManufacturer = "Radix",
            ManufacturerPartNumber = "MPN-100",
            ManufacturerRevision = "Rev A",
            ManufacturerSerialNumber = "MSN-100",
            PackingList = "PL-100",
            CheckedBy = "QA Team"
        };

        Assert.Equal("Production", request.AssembledIn);
        Assert.Equal("Assembly Team", request.AssembledBy);
        Assert.Equal(new DateOnly(2026, 6, 24), request.DateAssembled);
        Assert.Equal("Radix", request.HardwareManufacturer);
        Assert.Equal("MPN-100", request.ManufacturerPartNumber);
        Assert.Equal("Rev A", request.ManufacturerRevision);
        Assert.Equal("MSN-100", request.ManufacturerSerialNumber);
        Assert.Equal("PL-100", request.PackingList);
        Assert.Equal("QA Team", request.CheckedBy);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateBuildDetailsResult.Failure("Build Record was not found.");

        Assert.False(result.Succeeded);
        Assert.Equal(["Build Record was not found."], result.Errors);
    }
}
