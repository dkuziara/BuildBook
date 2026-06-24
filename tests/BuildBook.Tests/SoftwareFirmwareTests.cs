using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class SoftwareFirmwareTests
{
    [Fact]
    public void RequestStoresSoftwareFirmwareFields()
    {
        var request = new UpdateSoftwareFirmwareRequest
        {
            DiskImageVersion = "Image 1",
            RadSightVersion = "1.3.6",
            WindowsVersion = "Windows 10",
            WindowsLatestPatch = "KB100",
            BleuvioFirmwareVersion = "Bleuvio 2",
            CharthouseIrdaFirmwareVersion = "IRDA 3",
            RadioFirmware = "Radio 4"
        };

        Assert.Equal("Image 1", request.DiskImageVersion);
        Assert.Equal("1.3.6", request.RadSightVersion);
        Assert.Equal("Windows 10", request.WindowsVersion);
        Assert.Equal("KB100", request.WindowsLatestPatch);
        Assert.Equal("Bleuvio 2", request.BleuvioFirmwareVersion);
        Assert.Equal("IRDA 3", request.CharthouseIrdaFirmwareVersion);
        Assert.Equal("Radio 4", request.RadioFirmware);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateSoftwareFirmwareResult.Failure("Build Record was not found.");

        Assert.False(result.Succeeded);
        Assert.Equal(["Build Record was not found."], result.Errors);
    }
}
