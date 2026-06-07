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

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
