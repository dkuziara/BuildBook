namespace BuildBook.Tests;

public class DeploymentGuideTests
{
    [Fact]
    public void InternalDeploymentGuideDocumentsIisWindowsAuthenticationAndBootstrapAdministrators()
    {
        var guidePath = Path.Combine(
            GetRepositoryRoot(),
            "docs",
            "internal-deployment-guide.md");

        Assert.True(File.Exists(guidePath), "The internal deployment guide should exist.");

        var guideContent = File.ReadAllText(guidePath);

        Assert.Contains("IIS", guideContent, StringComparison.Ordinal);
        Assert.Contains("Windows Authentication", guideContent, StringComparison.Ordinal);
        Assert.Contains("BuildBook:Authorization:BootstrapAdministrators", guideContent, StringComparison.Ordinal);
        Assert.Contains("BuildBook:DataProtectionKeyDirectory", guideContent, StringComparison.Ordinal);
        Assert.Contains("BuildBookDatabase", guideContent, StringComparison.Ordinal);
        Assert.Contains("EnableDetailedErrors", guideContent, StringComparison.Ordinal);
        Assert.Contains("SeedDevelopmentData", guideContent, StringComparison.Ordinal);
        Assert.Contains("There is no BuildBook username/password store.", guideContent, StringComparison.Ordinal);
        Assert.Contains("Admin > Users & Roles", guideContent, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
