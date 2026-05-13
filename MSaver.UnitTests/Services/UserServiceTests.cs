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

        UserRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
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