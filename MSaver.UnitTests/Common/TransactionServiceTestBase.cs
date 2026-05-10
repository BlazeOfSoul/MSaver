using MSaver.Application.Abstractions.Services;

namespace MSaver.UnitTests.Common;

public abstract class TransactionServiceTestBase
{
    protected Mock<IUserRepository> UserRepositoryMock { get; } = new();
    protected Mock<IAccountRepository> AccountRepositoryMock { get; } = new();
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; } = new();
    protected Mock<ITransactionRepository> TransactionRepositoryMock { get; } = new();
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();
    protected Mock<ICurrentUserService> CurrentUserServiceMock { get; } = new();
    protected Mock<IExchangeRateService> ExchangeRateServiceMock { get; } = new();

    protected TransactionService CreateSut()
    {
        return new TransactionService(
            UserRepositoryMock.Object,
            AccountRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            TransactionRepositoryMock.Object,
            UnitOfWorkMock.Object,
            CurrentUserServiceMock.Object,
            ExchangeRateServiceMock.Object);
    }
}