namespace BuildBook.Tests;

public class OrdersDocumentationTests
{
    [Fact]
    public void ReadmeLinksToOrdersSpecificationAndBacklog()
    {
        var readmeContent = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        Assert.Contains("BuildBook Orders Module Specification", readmeContent, StringComparison.Ordinal);
        Assert.Contains("docs/specs/BuildBook-Orders-Module-Specification.md", readmeContent, StringComparison.Ordinal);
        Assert.Contains("docs/backlog/BuildBook-Orders-Module-Backlog.md", readmeContent, StringComparison.Ordinal);
    }

    [Fact]
    public void AgentsInstructionsDirectOrdersWorkToOrdersSpecification()
    {
        var agentsContent = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "AGENTS.md"));

        Assert.Contains("For Orders module work, also read:", agentsContent, StringComparison.Ordinal);
        Assert.Contains("docs/specs/BuildBook-Orders-Module-Specification.md", agentsContent, StringComparison.Ordinal);
    }

    [Fact]
    public void OrdersSpecificationUsesSyntheticExamplesInsteadOfRealCustomerData()
    {
        var specificationContent = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "docs",
            "specs",
            "BuildBook-Orders-Module-Specification.md"));

        Assert.Contains("Example Customer", specificationContent, StringComparison.Ordinal);
        Assert.Contains("Demo Site", specificationContent, StringComparison.Ordinal);
        Assert.Contains("Internal Test Customer", specificationContent, StringComparison.Ordinal);
        Assert.Contains("ORDER-DEMO-001", specificationContent, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
