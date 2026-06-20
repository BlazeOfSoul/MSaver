namespace MSaver.UnitTests.Domain;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldStoreUtcExpiresAt_WhenDateKindIsUnspecified()
    {
        var expiresAt = new DateTime(2026, 6, 21, 10, 0, 0, DateTimeKind.Unspecified);

        var token = RefreshToken.Create(
            TransactionTestData.UserId,
            "client",
            "token",
            expiresAt);

        token.ExpiresAt.Kind.Should().Be(DateTimeKind.Utc);
        token.ExpiresAt.Should().Be(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc));
    }

    [Fact]
    public void Replace_ShouldStoreUtcExpiresAt_WhenDateKindIsUnspecified()
    {
        var token = RefreshToken.Create(
            TransactionTestData.UserId,
            "client",
            "token",
            DateTime.UtcNow.AddDays(1));
        var expiresAt = new DateTime(2026, 6, 22, 10, 0, 0, DateTimeKind.Unspecified);

        token.Replace("new-token", expiresAt);

        token.ExpiresAt.Kind.Should().Be(DateTimeKind.Utc);
        token.ExpiresAt.Should().Be(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc));
    }
}
