namespace BuildBook.Tests;

public class GitHubActionsWorkflowTests
{
    [Fact]
    public void PullRequestWorkflowBuildsSolutionAndRunsStableTestSuite()
    {
        var workflowPath = Path.Combine(
            GetRepositoryRoot(),
            ".github",
            "workflows",
            "pull-request-build.yml");

        Assert.True(File.Exists(workflowPath), "The pull request GitHub Actions workflow should exist.");

        var workflowContent = File.ReadAllText(workflowPath);

        Assert.Contains("pull_request:", workflowContent, StringComparison.Ordinal);
        Assert.Contains("runs-on: windows-latest", workflowContent, StringComparison.Ordinal);
        Assert.Contains("dotnet-version: 10.0.x", workflowContent, StringComparison.Ordinal);
        Assert.Contains("dotnet restore BuildBook.sln", workflowContent, StringComparison.Ordinal);
        Assert.Contains("dotnet build BuildBook.sln --configuration Release --no-restore -m:1 -nr:false", workflowContent, StringComparison.Ordinal);
        Assert.Contains("dotnet test tests/BuildBook.Tests/BuildBook.Tests.csproj --configuration Release --no-build", workflowContent, StringComparison.Ordinal);
        Assert.Contains("FullyQualifiedName!~BuildRecordSmokeTests", workflowContent, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
