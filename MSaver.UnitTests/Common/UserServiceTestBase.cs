namespace MSaver.UnitTests.Common;

public abstract class UserServiceTestBase
{
    protected Mock<IUserRepository> UserRepositoryMock { get; } = new();
    protected Mock<ICurrentUserService> CurrentUserServiceMock { get; } = new();
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();

    protected UserService CreateSut()
    {
        return new UserService(
            UserRepositoryMock.Object,
            CurrentUserServiceMock.Object,
            UnitOfWorkMock.Object);
    }
}
