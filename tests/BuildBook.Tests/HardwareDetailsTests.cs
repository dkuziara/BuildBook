using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class HardwareDetailsTests
{
    [Fact]
    public void RequestStoresHardwareDetailsFields()
    {
        var request = new UpdateHardwareDetailsRequest
        {
            PanelDeviceModel = "Panel 100",
            PanelDeviceSerial = "PANEL-100",
            PanelFirmwareVersion = "1.2.3",
            MachineName = "RADSIGHT-100",
            RadioSerialNumber = "RADIO-100",
            HardwareNotes = "Checked connections."
        };

        Assert.Equal("Panel 100", request.PanelDeviceModel);
        Assert.Equal("PANEL-100", request.PanelDeviceSerial);
        Assert.Equal("1.2.3", request.PanelFirmwareVersion);
        Assert.Equal("RADSIGHT-100", request.MachineName);
        Assert.Equal("RADIO-100", request.RadioSerialNumber);
        Assert.Equal("Checked connections.", request.HardwareNotes);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateHardwareDetailsResult.Failure("Build Record was not found.");

        Assert.False(result.Succeeded);
        Assert.Equal(["Build Record was not found."], result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void SuccessResultCanReturnDuplicateMachineNameWarning()
    {
        var result = UpdateHardwareDetailsResult.Success("Another Build Record already uses this machine name.");

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.Equal(["Another Build Record already uses this machine name."], result.Warnings);
    }
}
