namespace MSaver.UnitTests.Api;

public sealed class CiWorkflowTests
{
    [Fact]
    public void CiWorkflow_ShouldRunDeployReadinessChecks()
    {
        var workflow = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            ".github",
            "workflows",
            "ci.yml"));

        workflow.Should().Contain("dotnet format ./MSaver.sln --verify-no-changes");
        workflow.Should().Contain("package --vulnerable --include-transitive");
        workflow.Should().Contain("docker build");
    }

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
