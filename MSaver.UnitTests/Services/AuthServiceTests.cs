using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;

using MSaver.Domain.Enums;
using MSaver.UnitTests.Common;
using MSaver.UnitTests.Common.TestData;

namespace MSaver.UnitTests.Services;

public sealed class AuthServiceTests : AuthServiceTestBase
{
    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserWithEmailWasNotFound()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateLoginRequest();

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.LoginAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.InvalidEmail);

        PasswordHasherMock.Verify(
            x => x.VerifyHashedPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteExpiredByUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateLoginRequest();
        var user = AuthTestData.CreateUser(
            email: request.Email,
            passwordHash: "stored-hash");

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        PasswordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Failed);

        var result = await sut.LoginAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.InvalidPassword);

        JwtTokenGeneratorMock.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        JwtTokenGeneratorMock.Verify(
            x => x.GenerateRefreshToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokensAndClientId_WhenCredentialsAreValid()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateLoginRequest();
        var user = AuthTestData.CreateUser(
            email: request.Email,
            passwordHash: "stored-hash");

        var refreshExpiresAt = new DateTime(2026, 06, 01, 0, 0, 0, DateTimeKind.Utc);
        string? capturedClientId = null;
        RefreshToken? createdRefreshToken = null;

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        PasswordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Success);

        RefreshTokenRepositoryMock
            .Setup(x => x.DeleteExpiredByUserAsync(user.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(
                user.Id,
                user.Name,
                user.Email,
                It.IsAny<string>()))
            .Callback<Guid, string, string, string>((_, _, _, clientId) => capturedClientId = clientId)
            .Returns("access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(
                user.Id,
                user.Name,
                user.Email,
                It.IsAny<string>()))
            .Returns<Guid, string, string, string>((_, _, _, clientId) =>
            {
                capturedClientId ??= clientId;
                return ("refresh-token", refreshExpiresAt);
            });

        RefreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => createdRefreshToken = token)
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.LoginAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id);
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.ClientId.Should().NotBeNullOrWhiteSpace();
        result.Value.ClientId.Should().HaveLength(32);

        capturedClientId.Should().Be(result.Value.ClientId);

        createdRefreshToken.Should().NotBeNull();
        createdRefreshToken!.UserId.Should().Be(user.Id);
        createdRefreshToken.ClientId.Should().Be(result.Value.ClientId);
        createdRefreshToken.Token.Should().Be(TokenHash("refresh-token"));
        createdRefreshToken.Token.Should().NotBe("refresh-token");
        createdRefreshToken.ExpiresAt.Should().Be(refreshExpiresAt);

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteExpiredByUserAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldDeleteOldestActiveSession_WhenUserAlreadyHasThreeActiveSessions()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateLoginRequest();
        var user = AuthTestData.CreateUser(
            email: request.Email,
            passwordHash: "stored-hash");

        var firstToken = AuthTestData.CreateActiveRefreshToken(
            user.Id,
            "client-1",
            "token-1",
            DateTime.UtcNow.AddDays(10));

        await Task.Delay(5);

        var secondToken = AuthTestData.CreateActiveRefreshToken(
            user.Id,
            "client-2",
            "token-2",
            DateTime.UtcNow.AddDays(11));

        await Task.Delay(5);

        var thirdToken = AuthTestData.CreateActiveRefreshToken(
            user.Id,
            "client-3",
            "token-3",
            DateTime.UtcNow.AddDays(12));

        var existingTokens = new[] { firstToken, secondToken, thirdToken };
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        PasswordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Success);

        RefreshTokenRepositoryMock
            .Setup(x => x.DeleteExpiredByUserAsync(user.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTokens);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Name, user.Email, It.IsAny<string>()))
            .Returns("access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(user.Id, user.Name, user.Email, It.IsAny<string>()))
            .Returns(("new-refresh-token", refreshExpiresAt));

        RefreshTokenRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RefreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.LoginAsync(request);

        result.IsSuccess.Should().BeTrue();

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteAsync(firstToken, It.IsAny<CancellationToken>()),
            Times.Once);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRegisterRequest();
        var existingUser = AuthTestData.CreateUser(email: request.Email);

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var result = await sut.RegisterAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.RepeatedEmail);

        PasswordHasherMock.Verify(
            x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never);

        UserRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);

        CategoryRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<Category>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndDefaultCategories_WhenEmailIsUnique()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRegisterRequest();

        User? createdUser = null;
        List<Category>? createdCategories = null;

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        PasswordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), request.Password))
            .Returns("hashed-password");

        UserRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        CategoryRepositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Category>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Category>, CancellationToken>((categories, _) =>
                createdCategories = categories.ToList())
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        createdUser.Should().NotBeNull();
        createdUser!.Name.Should().Be(request.Username);
        createdUser.Email.Should().Be(request.Email);
        createdUser.PasswordHash.Should().Be("hashed-password");

        createdCategories.Should().NotBeNull();
        createdCategories.Should().NotBeEmpty();
        createdCategories!.Single(x => x.Name == "Продукты").Color.Should().Be("#F97373");
        createdCategories.Single(x => x.Name == "Кафе и рестораны").Color.Should().Be("#FB923C");
        createdCategories.Single(x => x.Name == "Для дома (Интерьер)").Color.Should().Be("#818CF8");
        createdCategories.Single(x => x.Name == "Для дома (Бытовое)").Color.Should().Be("#38BDF8");
        createdCategories.Single(x => x.Name == "Проезд (Метро и Автобусы)").Color.Should().Be("#22D3EE");
        createdCategories.Single(x => x.Name == "Проезд (Каршеринг и такси)").Color.Should().Be("#0EA5E9");
        createdCategories.Single(x => x.Name == "Зарплата").Color.Should().Be("#22C55E");
        createdCategories.Single(x => x.Name == "Взято в долг (+)").DefaultCategoryType.Should().Be(DefaultCategoryType.DebtTaken);
        createdCategories.Single(x => x.Name == "Возвращено по долгу (-)").DefaultCategoryType.Should().Be(DefaultCategoryType.DebtReturned);
        createdCategories.Single(x => x.Name == "Дано в долг (-)").DefaultCategoryType.Should().Be(DefaultCategoryType.DebtGiven);
        createdCategories.Single(x => x.Name == "Отдано по долгу (+)").DefaultCategoryType.Should().Be(DefaultCategoryType.DebtPaidBack);

        CategoryRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<Category>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnFailure_WhenRefreshTokenWasNotFound()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest();

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await sut.RefreshAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.RefreshTokenInvalid);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldDeleteExpiredTokenAndReturnFailure_WhenRefreshTokenIsExpired()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest();
        var user = AuthTestData.CreateUser();
        var expiredToken = AuthTestData.CreateExpiredRefreshToken(user.Id, "client-1", request.RefreshToken);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        RefreshTokenRepositoryMock
            .Setup(x => x.DeleteAsync(expiredToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.RefreshAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.RefreshTokenExpired);

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteAsync(expiredToken, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnFailure_WhenUserForTokenWasNotFound()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest("refresh-token");
        var userId = Guid.NewGuid();
        var storedToken = AuthTestData.CreateActiveRefreshToken(userId, "client-1", request.RefreshToken);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.RefreshAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.InvalidEmail);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReplaceStoredTokenAndReturnNewTokens_WhenRefreshTokenIsValid()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest("old-refresh-token");
        var user = AuthTestData.CreateUser();
        var storedToken = AuthTestData.CreateActiveRefreshToken(
            user.Id,
            "client-1",
            request.RefreshToken,
            DateTime.UtcNow.AddDays(10));

        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Name, user.Email, storedToken.ClientId))
            .Returns("new-access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(user.Id, user.Name, user.Email, storedToken.ClientId))
            .Returns(("new-refresh-token", newRefreshExpiresAt));

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var oldTokenValue = storedToken.Token;
        oldTokenValue.Should().Be(TokenHash(request.RefreshToken));

        var result = await sut.RefreshAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id);
        result.Value.Name.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Email);
        result.Value.ClientId.Should().Be(storedToken.ClientId);
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");

        storedToken.Token.Should().NotBe(oldTokenValue);
        storedToken.Token.Should().Be(TokenHash("new-refresh-token"));
        storedToken.Token.Should().NotBe("new-refresh-token");
        storedToken.ExpiresAt.Should().Be(newRefreshExpiresAt);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutClientAsync_ShouldReturnSuccess_WhenSessionWasNotFound()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var clientId = "client-1";

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByClientIdAsync(userId, clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await sut.LogoutClientAsync(userId, clientId);

        result.IsSuccess.Should().BeTrue();

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutClientAsync_ShouldDeleteSession_WhenSessionWasFound()
    {
        var sut = CreateSut();
        var user = AuthTestData.CreateUser();
        var session = AuthTestData.CreateActiveRefreshToken(user.Id, "client-1", "refresh-token");

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByClientIdAsync(user.Id, "client-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        RefreshTokenRepositoryMock
            .Setup(x => x.DeleteAsync(session, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.LogoutClientAsync(user.Id, "client-1");

        result.IsSuccess.Should().BeTrue();

        RefreshTokenRepositoryMock.Verify(
            x => x.DeleteAsync(session, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAllAsync_ShouldDeleteAllSessions()
    {
        var sut = CreateSut();
        var user = AuthTestData.CreateUser();

        var sessions = new[]
        {
            AuthTestData.CreateActiveRefreshToken(user.Id, "client-1", "token-1"),
            AuthTestData.CreateActiveRefreshToken(user.Id, "client-2", "token-2"),
            AuthTestData.CreateActiveRefreshToken(user.Id, "client-3", "token-3")
        };

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        RefreshTokenRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.LogoutAllAsync(user.Id);

        result.IsSuccess.Should().BeTrue();

        foreach (var session in sessions)
        {
            RefreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(session, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static string TokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
