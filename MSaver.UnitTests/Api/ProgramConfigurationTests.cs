namespace MSaver.UnitTests.Api;

public sealed class ProgramConfigurationTests
{
    [Fact]
    public void Program_ShouldConfigureSerilogHostAndRequestLogging()
    {
        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "Program.cs"));
        var appSettings = File.ReadAllText(Path.Combine(RepositoryRoot, "appsettings.json"));

        program.Should().Contain("UseSerilog(");
        program.Should().Contain("UseSerilogRequestLogging(");
        appSettings.Should().Contain("\"Serilog\"");
    }

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
