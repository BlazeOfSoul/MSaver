using FluentAssertions;

using Microsoft.AspNetCore.Identity;

using Moq;

using MSaver.Domain.Entities;
using MSaver.Domain.Errors;
using MSaver.UnitTests.Common;
using MSaver.UnitTests.Common.TestData;

using Xunit;

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
            x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateLoginRequest();
        var user = AuthTestData.CreateUser(
            email: request.Email,
            passwordHash: "stored-hash");

        var refreshExpiresAt = new DateTime(2026, 06, 01, 0, 0, 0, DateTimeKind.Utc);

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        PasswordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Success);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Name, user.Email))
            .Returns("access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(user.Id, user.Name, user.Email))
            .Returns(("refresh-token", refreshExpiresAt));

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        RefreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
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

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldRevokeOldestActiveRefreshToken_WhenUserAlreadyHasThreeActiveTokens()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateLoginRequest();
        var user = AuthTestData.CreateUser(
            email: request.Email,
            passwordHash: "stored-hash");

        var firstToken = AuthTestData.CreateActiveRefreshToken(user.Id, "token-1", DateTime.UtcNow.AddDays(10));
        await Task.Delay(5);
        var secondToken = AuthTestData.CreateActiveRefreshToken(user.Id, "token-2", DateTime.UtcNow.AddDays(11));
        await Task.Delay(5);
        var thirdToken = AuthTestData.CreateActiveRefreshToken(user.Id, "token-3", DateTime.UtcNow.AddDays(12));

        var existingTokens = new[] { firstToken, secondToken, thirdToken };
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        PasswordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Success);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Name, user.Email))
            .Returns("access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(user.Id, user.Name, user.Email))
            .Returns(("new-refresh-token", newRefreshExpiresAt));

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTokens);

        RefreshTokenRepositoryMock
            .Setup(x => x.RevokeAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
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
            x => x.RevokeAsync(firstToken, It.IsAny<CancellationToken>()),
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
    public async Task RefreshAsync_ShouldReturnFailure_WhenRefreshTokenIsExpired()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest();
        var user = AuthTestData.CreateUser();
        var expiredToken = AuthTestData.CreateExpiredRefreshToken(user.Id, request.RefreshToken);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var result = await sut.RefreshAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.RefreshTokenExpired);

        RefreshTokenRepositoryMock.Verify(
            x => x.RevokeAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnFailure_WhenUserForTokenWasNotFound()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest("refresh-token");
        var userId = Guid.NewGuid();
        var storedToken = AuthTestData.CreateActiveRefreshToken(userId, request.RefreshToken);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.RefreshAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthDomainErrors.InvalidEmail);

        RefreshTokenRepositoryMock.Verify(
            x => x.RevokeAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest("old-refresh-token");
        var user = AuthTestData.CreateUser();
        var storedToken = AuthTestData.CreateActiveRefreshToken(user.Id, request.RefreshToken);
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        RefreshTokenRepositoryMock
            .Setup(x => x.RevokeAsync(storedToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Name, user.Email))
            .Returns("new-access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(user.Id, user.Name, user.Email))
            .Returns(("new-refresh-token", newRefreshExpiresAt));

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        RefreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.RefreshAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id);
        result.Value.Username.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Email);
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");

        RefreshTokenRepositoryMock.Verify(
            x => x.RevokeAsync(storedToken, It.IsAny<CancellationToken>()),
            Times.Once);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRevokeOldestActiveRefreshToken_WhenUserAlreadyHasThreeActiveTokens()
    {
        var sut = CreateSut();
        var request = AuthTestData.CreateRefreshTokenRequest("old-refresh-token");
        var user = AuthTestData.CreateUser();
        var storedToken = AuthTestData.CreateActiveRefreshToken(user.Id, request.RefreshToken);

        var firstToken = AuthTestData.CreateActiveRefreshToken(user.Id, "token-1", DateTime.UtcNow.AddDays(5));
        await Task.Delay(5);
        var secondToken = AuthTestData.CreateActiveRefreshToken(user.Id, "token-2", DateTime.UtcNow.AddDays(6));
        await Task.Delay(5);
        var thirdToken = AuthTestData.CreateActiveRefreshToken(user.Id, "token-3", DateTime.UtcNow.AddDays(7));

        var existingTokens = new[] { firstToken, secondToken, thirdToken };
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        RefreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        RefreshTokenRepositoryMock
            .Setup(x => x.RevokeAsync(storedToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Name, user.Email))
            .Returns("new-access-token");

        JwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(user.Id, user.Name, user.Email))
            .Returns(("new-refresh-token", newRefreshExpiresAt));

        RefreshTokenRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTokens);

        RefreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.RefreshAsync(request);

        result.IsSuccess.Should().BeTrue();

        RefreshTokenRepositoryMock.Verify(
            x => x.RevokeAsync(firstToken, It.IsAny<CancellationToken>()),
            Times.Once);

        RefreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}