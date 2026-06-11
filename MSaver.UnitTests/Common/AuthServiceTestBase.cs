using Microsoft.Extensions.Logging.Abstractions;

namespace MSaver.UnitTests.Common;

public abstract class AuthServiceTestBase
{
    protected readonly Mock<IUserRepository> UserRepositoryMock = new();
    protected readonly Mock<ICategoryRepository> CategoryRepositoryMock = new();
    protected readonly Mock<IRefreshTokenRepository> RefreshTokenRepositoryMock = new();
    protected readonly Mock<IJwtTokenGenerator> JwtTokenGeneratorMock = new();
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock = new();
    protected readonly Mock<IPasswordHasher<User>> PasswordHasherMock = new();

    protected AuthService CreateSut()
    {
        return new AuthService(
            UserRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            RefreshTokenRepositoryMock.Object,
            JwtTokenGeneratorMock.Object,
            UnitOfWorkMock.Object,
            PasswordHasherMock.Object,
            NullLogger<AuthService>.Instance);
    }
}
