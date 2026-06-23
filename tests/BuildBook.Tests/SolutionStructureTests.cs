namespace BuildBook.Tests;

public class SolutionStructureTests
{
    [Fact]
    public void ApplicationAndDomainProjectsAreReferenced()
    {
        Assert.Equal("BuildBook.Application", typeof(BuildBook.Application.AssemblyMarker).Namespace);
        Assert.Equal("BuildBook.Domain", typeof(BuildBook.Domain.AssemblyMarker).Namespace);
    }
}
