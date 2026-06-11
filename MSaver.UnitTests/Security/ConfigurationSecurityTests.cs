using System.Text.Json;

namespace MSaver.UnitTests.Security;

public sealed class ConfigurationSecurityTests
{
    public static TheoryData<string> AppSettingsFiles => new()
    {
        "appsettings.json",
        "appsettings.Development.json"
    };

    [Theory]
    [MemberData(nameof(AppSettingsFiles))]
    public void AppSettings_ShouldNotContainCommittedSecrets(string fileName)
    {
        var path = Path.Combine(RepositoryRoot, fileName);
        var content = File.ReadAllText(path);

        content.Should().NotContain("this is my custom secret key for authentication");
        content.Should().NotContain("be629b6a418691a888a9ba41");
        content.Should().NotContain("Password=postgres");
    }

    [Fact]
    public void LaunchSettings_HttpProfileShouldMatchFrontendProxyTarget()
    {
        var path = Path.Combine(RepositoryRoot, "Properties", "launchSettings.json");
        var content = File.ReadAllText(path);

        using var document = JsonDocument.Parse(content);

        var httpProfile = document.RootElement
            .GetProperty("profiles")
            .GetProperty("http");

        httpProfile
            .GetProperty("applicationUrl")
            .GetString()
            .Should()
            .Be("http://localhost:5200");
    }

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
