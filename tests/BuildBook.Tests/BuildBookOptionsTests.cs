using BuildBook.Web.Configuration;

namespace BuildBook.Tests;

public class BuildBookOptionsTests
{
    [Fact]
    public void DefaultOptionsAreValid()
    {
        var options = new BuildBookOptions();

        Assert.True(options.IsValid());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void OptionsRequireApplicationName(string applicationName)
    {
        var options = new BuildBookOptions
        {
            ApplicationName = applicationName
        };

        Assert.False(options.IsValid());
    }

    [Theory]
    [InlineData(9)]
    [InlineData(101)]
    public void OptionsRequireReasonableDefaultPageSize(int defaultPageSize)
    {
        var options = new BuildBookOptions
        {
            DefaultPageSize = defaultPageSize
        };

        Assert.False(options.IsValid());
    }
}
