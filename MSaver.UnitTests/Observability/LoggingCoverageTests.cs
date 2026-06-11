namespace MSaver.UnitTests.Observability;

public sealed class LoggingCoverageTests
{
    [Fact]
    public void AuthService_ShouldLogImportantAuthenticationEvents()
    {
        var source = ReadSource("Application", "Services", "AuthService.cs");

        source.Should().Contain("ILogger<AuthService>");
        source.Should().Contain("Login rejected because user was not found");
        source.Should().Contain("Login rejected because password verification failed");
        source.Should().Contain("User logged in");
        source.Should().Contain("User registered");
        source.Should().Contain("Refresh token rejected because it was not found");
        source.Should().Contain("Expired refresh token removed");
        source.Should().Contain("User logged out client session");
        source.Should().Contain("User logged out all sessions");
    }

    [Fact]
    public void ExchangeRateApiService_ShouldLogExternalProviderAndCacheEvents()
    {
        var source = ReadSource("Infrastructure", "Services", "ExchangeRateApiService.cs");

        source.Should().Contain("ILogger<ExchangeRateApiService>");
        source.Should().Contain("Exchange rate cache hit");
        source.Should().Contain("Fetching exchange rate");
        source.Should().Contain("Exchange rate provider returned HTTP error");
        source.Should().Contain("Exchange rate fetched and cached");
    }

    private static string ReadSource(params string[] paths) =>
        File.ReadAllText(Path.Combine(RepositoryRoot, Path.Combine(paths)));

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
