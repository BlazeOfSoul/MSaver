using MSaver.Application.Features.Users.UpdateApplicationCurrency;
using MSaver.UnitTests.Common;

namespace MSaver.UnitTests.Services;

public sealed class UserServiceTests : UserServiceTestBase
{
    [Fact]
    public async Task GetCurrentAsync_ShouldReturnFailure_WhenUserWasNotFound()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.GetCurrentAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserDomainErrors.UserNotFound);

        UserRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldReturnCurrentUserResponse_WhenUserExists()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();

        var user = User.Create(
            name: "Alex",
            email: "alex@example.com",
            passwordHash: "hashed-password");

        SetId(user, userId);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await sut.GetCurrentAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Id.Should().Be(userId);
        result.Value.Username.Should().Be("Alex");
        result.Value.Email.Should().Be("alex@example.com");
        result.Value.ApplicationCurrencyCode.Should().Be("BYN");

        UserRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateApplicationCurrencyAsync_ShouldReturnFailure_WhenUserWasNotFound()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var request = new UpdateApplicationCurrencyRequest("EUR");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.UpdateApplicationCurrencyAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserDomainErrors.UserNotFound);

        UserRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateApplicationCurrencyAsync_ShouldReturnFailure_WhenCurrencyIsUnsupported()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var user = User.Create("Alex", "alex@example.com", "hashed-password");
        var request = new UpdateApplicationCurrencyRequest("XYZ");

        SetId(user, userId);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await sut.UpdateApplicationCurrencyAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.CurrencyNotFound);

        UserRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateApplicationCurrencyAsync_ShouldPersistNormalizedCurrency_WhenCurrencyIsSupported()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var user = User.Create("Alex", "alex@example.com", "hashed-password");
        var request = new UpdateApplicationCurrencyRequest(" eur ");

        SetId(user, userId);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        UserRepositoryMock
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.UpdateApplicationCurrencyAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ApplicationCurrencyCode.Should().Be("EUR");
        user.ApplicationCurrencyCode.Should().Be("EUR");

        UserRepositoryMock.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static void SetId(object entity, Guid id)
    {
        var type = entity.GetType();

        while (type is not null)
        {
            var property = type.GetProperty(
                "Id",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (property is not null)
            {
                property.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }
    }
}
