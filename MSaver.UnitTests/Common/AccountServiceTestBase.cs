namespace MSaver.UnitTests.Common;

public abstract class AccountServiceTestBase
{
    protected readonly Mock<IAccountRepository> AccountRepositoryMock = new();
    protected readonly Mock<ITransactionRepository> TransactionRepositoryMock = new();
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock = new();
    protected readonly Mock<ICurrentUserService> CurrentUserServiceMock = new();

    protected AccountService CreateSut()
    {
        return new AccountService(
            AccountRepositoryMock.Object,
            TransactionRepositoryMock.Object,
            UnitOfWorkMock.Object,
            CurrentUserServiceMock.Object);
    }
}