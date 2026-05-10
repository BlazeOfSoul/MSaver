namespace MSaver.UnitTests.Common;

public abstract class CategoryServiceTestBase
{
    protected readonly Mock<ICategoryRepository> CategoryRepositoryMock = new();
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock = new();
    protected readonly Mock<ICurrentUserService> CurrentUserServiceMock = new();

    protected CategoryService CreateSut()
    {
        return new CategoryService(
            CategoryRepositoryMock.Object,
            UnitOfWorkMock.Object,
            CurrentUserServiceMock.Object);
    }
}