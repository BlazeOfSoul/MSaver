namespace MSaver.UnitTests.Common;

public abstract class TagServiceTestBase
{
    protected Mock<IUserRepository> UserRepositoryMock { get; } = new();
    protected Mock<ITagRepository> TagRepositoryMock { get; } = new();
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; } = new();
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();
    protected Mock<ICurrentUserService> CurrentUserServiceMock { get; } = new();

    protected TagService CreateSut()
    {
        return new TagService(
            UserRepositoryMock.Object,
            TagRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            UnitOfWorkMock.Object,
            CurrentUserServiceMock.Object);
    }
}