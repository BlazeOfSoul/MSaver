using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Repositories;

public sealed class RefreshTokenRepositoryTests
{
    [Fact]
    public async Task GetByTokenAsync_ShouldFindTokenByRawValue_WhenStoredTokenIsHashed()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        const string rawToken = "refresh-token-value";
        var refreshToken = RefreshToken.Create(
            user.Id,
            "client-1",
            rawToken,
            DateTime.UtcNow.AddDays(7));

        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persistedToken = await context.RefreshTokens.SingleAsync();
        persistedToken.Token.Should().Be(TokenHash(rawToken));
        persistedToken.Token.Should().NotBe(rawToken);

        var repository = new RefreshTokenRepository(context);

        var result = await repository.GetByTokenAsync(rawToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(refreshToken.Id);
        result.Token.Should().Be(TokenHash(rawToken));
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldNotTreatStoredHashAsUsableRefreshToken()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        const string rawToken = "refresh-token-value";
        var refreshToken = RefreshToken.Create(
            user.Id,
            "client-1",
            rawToken,
            DateTime.UtcNow.AddDays(7));

        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var storedHash = TokenHash(rawToken);
        var repository = new RefreshTokenRepository(context);

        var result = await repository.GetByTokenAsync(storedHash);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteExpiredByUserAsync_ShouldStageDeletesUntilUnitOfWorkSaves()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var expired = RefreshToken.Create(
            user.Id,
            "client-expired",
            "expired-refresh-token",
            DateTime.UtcNow.AddDays(-1));
        var active = RefreshToken.Create(
            user.Id,
            "client-active",
            "active-refresh-token",
            DateTime.UtcNow.AddDays(7));

        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddRangeAsync(expired, active);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RefreshTokenRepository(context);

        await repository.DeleteExpiredByUserAsync(user.Id);

        context.ChangeTracker
            .Entries<RefreshToken>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => entry.Entity.Id)
            .Should()
            .ContainSingle(id => id == expired.Id);

        context.ChangeTracker
            .Entries<RefreshToken>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => entry.Entity.Id)
            .Should()
            .NotContain(active.Id);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string TokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
