namespace MSaver.UnitTests.Common.TestData;

public static class AuthTestData
{
    public static LoginRequest CreateLoginRequest(
        string email = "rostik@example.com",
        string password = "Password123!")
    {
        return new LoginRequest(email, password);
    }

    public static RegisterRequest CreateRegisterRequest(
        string username = "rostik",
        string email = "rostik@example.com",
        string password = "Password123!")
    {
        return new RegisterRequest(username, email, password);
    }

    public static RefreshTokenRequest CreateRefreshTokenRequest(
        string refreshToken = "refresh-token-123")
    {
        return new RefreshTokenRequest(refreshToken);
    }

    public static User CreateUser(
        string username = "rostik",
        string email = "rostik@example.com",
        string passwordHash = "hashed-password")
    {
        return User.Create(username, email, passwordHash);
    }

    public static RefreshToken CreateActiveRefreshToken(
        Guid userId,
        string clientId = "client-1",
        string token = "refresh-token",
        DateTime? expiresAt = null)
    {
        return RefreshToken.Create(
            userId,
            clientId,
            token,
            expiresAt ?? DateTime.UtcNow.AddDays(7));
    }

    public static RefreshToken CreateExpiredRefreshToken(
        Guid userId,
        string clientId = "client-1",
        string token = "expired-refresh-token")
    {
        return RefreshToken.Create(
            userId,
            clientId,
            token,
            DateTime.UtcNow.AddMinutes(-1));
    }
}