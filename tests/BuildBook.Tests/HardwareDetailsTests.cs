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
            RouterUsed = "Router A",
            HardwareNotes = "Checked connections."
        };

        Assert.Equal("Panel 100", request.PanelDeviceModel);
        Assert.Equal("PANEL-100", request.PanelDeviceSerial);
        Assert.Equal("1.2.3", request.PanelFirmwareVersion);
        Assert.Equal("RADSIGHT-100", request.MachineName);
        Assert.Equal("RADIO-100", request.RadioSerialNumber);
        Assert.Equal("Router A", request.RouterUsed);
        Assert.Equal("Checked connections.", request.HardwareNotes);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateHardwareDetailsResult.Failure("A Build Record with this machine name already exists.");

        Assert.False(result.Succeeded);
        Assert.Equal(["A Build Record with this machine name already exists."], result.Errors);
    }
}
