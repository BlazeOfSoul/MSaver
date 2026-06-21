using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Domain.Enums;
using MSaver.UnitTests.Common;
using MSaver.UnitTests.Common.TestData;

namespace MSaver.UnitTests.Services;

public sealed class TransactionServiceTests : TransactionServiceTestBase
{
    [Fact]
    public async Task GetAsync_ShouldReturnMappedPagedTransactions_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var account = TransactionTestData.CreateAccount(userId, "USD", "Main", "#111111");
        var category = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#222222", isDeleted: true);

        var tx1 = TransactionTestData.CreateTransaction(userId, account.Id, category.Id, -10m, description: "Coffee", account: account, category: category);
        var tx2 = TransactionTestData.CreateTransaction(userId, account.Id, category.Id, -25m, description: "Lunch", account: account, category: category);

        var request = TransactionTestData.CreateGetTransactionsRequest(
            search: " food ",
            sortBy: TransactionSortFields.Date,
            sortDirection: ListQueryDefaults.SortDescending,
            page: 2,
            size: 5);

        var paged = TransactionTestData.CreatePagedTransactions(new[] { tx1, tx2 }, page: 2, size: 5, totalCount: 12);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await sut.GetAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Items.Should().HaveCount(2);
        response.Page.Should().Be(2);
        response.Size.Should().Be(5);
        response.TotalCount.Should().Be(12);
        response.TotalPages.Should().Be(paged.TotalPages);
        response.HasPreviousPage.Should().BeTrue();
        response.HasNextPage.Should().BeTrue();

        response.Items.First().Account.Name.Should().Be("Main");
        response.Items.First().Category.Name.Should().Be("Food");
        response.Items.First().Category.Type.Should().Be(CategoryType.Debit);
        response.Items.First().Category.IsDeleted.Should().BeTrue();
        response.Items.First().Amount.Should().Be(-10m);
        response.Items.First().TransferId.Should().BeNull();
        response.Items.First().Description.Should().Be("Coffee");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnTransferCounterparty_WhenTransactionBelongsToTransfer()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var toAccount = TransactionTestData.CreateAccount(userId, "USD", "Savings", "#222222");
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense, "#333333");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            fromAccount.Id,
            expenseCategory.Id,
            -100m,
            account: fromAccount,
            category: expenseCategory,
            transferId: transferId);

        var income = TransactionTestData.CreateTransaction(
            userId,
            toAccount.Id,
            incomeCategory.Id,
            100m,
            account: toAccount,
            category: incomeCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TransactionTestData.CreatePagedTransactions([expense]));
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdsWithDetailsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(transferId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, income]);

        var result = await sut.GetAsync(new GetTransactionsRequest());

        result.IsSuccess.Should().BeTrue();
        var response = result.Value!.Items.Should().ContainSingle().Subject;
        response.TransferCounterparty.Should().NotBeNull();
        response.TransferCounterparty!.Id.Should().Be(income.Id);
        response.TransferCounterparty.Amount.Should().Be(100m);
        response.TransferCounterparty.Account.Id.Should().Be(toAccount.Id);
        response.TransferCounterparty.Account.Name.Should().Be("Savings");
        response.TransferCounterparty.Account.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task GetAsync_ShouldNotReturnTransferCounterparty_WhenPeerBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var otherAccount = TransactionTestData.CreateAccount(TransactionTestData.AnotherUserId, "USD", "Other", "#222222");
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense, "#333333");
        var otherIncomeCategory = TransactionTestData.CreateCategory(TransactionTestData.AnotherUserId, "Transfer In", CategoryType.TransferIncome, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            fromAccount.Id,
            expenseCategory.Id,
            -100m,
            account: fromAccount,
            category: expenseCategory,
            transferId: transferId);

        var otherIncome = TransactionTestData.CreateTransaction(
            TransactionTestData.AnotherUserId,
            otherAccount.Id,
            otherIncomeCategory.Id,
            100m,
            account: otherAccount,
            category: otherIncomeCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TransactionTestData.CreatePagedTransactions([expense]));
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdsWithDetailsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(transferId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, otherIncome]);

        var result = await sut.GetAsync(new GetTransactionsRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle().Subject.TransferCounterparty.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldNotReturnTransferCounterparty_WhenTransferPairUsesOperationCategories()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var toAccount = TransactionTestData.CreateAccount(userId, "USD", "Savings", "#222222");
        var debitCategory = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#333333");
        var creditCategory = TransactionTestData.CreateCategory(userId, "Salary", CategoryType.Credit, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            fromAccount.Id,
            debitCategory.Id,
            -100m,
            account: fromAccount,
            category: debitCategory,
            transferId: transferId);

        var income = TransactionTestData.CreateTransaction(
            userId,
            toAccount.Id,
            creditCategory.Id,
            100m,
            account: toAccount,
            category: creditCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TransactionTestData.CreatePagedTransactions([expense]));
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdsWithDetailsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(transferId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, income]);

        var result = await sut.GetAsync(new GetTransactionsRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle().Subject.TransferCounterparty.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldNormalizeAndPassQueryFields_WhenBuildingQuery()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fromDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var toDate = new DateTime(2026, 2, 1, 18, 30, 0, DateTimeKind.Unspecified);

        var request = new GetTransactionsRequest
        {
            AccountId = accountId,
            CategoryId = categoryId,
            FromDate = fromDate,
            ToDate = toDate,
            Search = "  coffee  ",
            SortBy = "  amount  ",
            SortDirection = "ASC",
            Page = 3,
            Size = 15
        };

        TransactionListQuery? capturedQuery = null;

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TransactionListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(TransactionTestData.CreatePagedTransactions([], page: 3, size: 15, totalCount: 0));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.AccountId.Should().Be(accountId);
        capturedQuery.CategoryId.Should().Be(categoryId);
        capturedQuery.FromDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        capturedQuery.FromDate.Should().Be(DateTime.SpecifyKind(fromDate, DateTimeKind.Utc));
        capturedQuery.ToDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        capturedQuery.ToDate.Should().Be(DateTime.SpecifyKind(toDate, DateTimeKind.Utc));
        capturedQuery.Search.Should().Be("coffee");
        capturedQuery.SortBy.Should().Be("amount");
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortAscending);
        capturedQuery.Page.Should().Be(3);
        capturedQuery.Size.Should().Be(15);
    }

    [Fact]
    public async Task GetAsync_ShouldTreatDateOnlyToDateAsInclusiveDay_WhenBuildingQuery()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var toDate = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Unspecified);

        var request = new GetTransactionsRequest
        {
            ToDate = toDate
        };

        TransactionListQuery? capturedQuery = null;

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TransactionListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(TransactionTestData.CreatePagedTransactions([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.ToDate.Should().Be(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetAsync_ShouldUseDefaultSortingAndPaging_WhenRequestHasDefaults()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = new GetTransactionsRequest();

        TransactionListQuery? capturedQuery = null;

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetPagedWithDetailsAsync(It.IsAny<TransactionListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TransactionListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(TransactionTestData.CreatePagedTransactions([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Search.Should().BeNull();
        capturedQuery.SortBy.Should().Be(TransactionSortFields.Date);
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortDescending);
        capturedQuery.Page.Should().Be(ListQueryDefaults.DefaultPage);
        capturedQuery.Size.Should().Be(ListQueryDefaults.DefaultPageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenTransactionWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransactionNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenTransactionBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var account = TransactionTestData.CreateAccount(TransactionTestData.AnotherUserId);
        var category = TransactionTestData.CreateCategory(TransactionTestData.AnotherUserId);
        var transaction = TransactionTestData.CreateTransaction(TransactionTestData.AnotherUserId, account.Id, category.Id, account: account, category: category);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransactionNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedTransaction_WhenTransactionExists()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var account = TransactionTestData.CreateAccount(userId, "EUR", "Wallet", "#123456");
        var category = TransactionTestData.CreateCategory(userId, "Salary", CategoryType.Credit, "#654321", isDeleted: true);
        var transaction = TransactionTestData.CreateTransaction(userId, account.Id, category.Id, 120m, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), "Salary April", transactionId, account, category);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Id.Should().Be(transaction.Id);
        response.Account.Name.Should().Be("Wallet");
        response.Account.CurrencyCode.Should().Be("EUR");
        response.Category.Name.Should().Be("Salary");
        response.Category.Type.Should().Be(CategoryType.Credit);
        response.Category.IsDeleted.Should().BeTrue();
        response.Amount.Should().Be(120m);
        response.TransferId.Should().BeNull();
        response.Description.Should().Be("Salary April");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTransferCounterparty_WhenTransactionBelongsToTransfer()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var toAccount = TransactionTestData.CreateAccount(userId, "EUR", "Euro Savings", "#222222");
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense, "#333333");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            fromAccount.Id,
            expenseCategory.Id,
            -100m,
            id: transactionId,
            account: fromAccount,
            category: expenseCategory,
            transferId: transferId);

        var income = TransactionTestData.CreateTransaction(
            userId,
            toAccount.Id,
            incomeCategory.Id,
            91m,
            account: toAccount,
            category: incomeCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, income]);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value!;
        response.TransferCounterparty.Should().NotBeNull();
        response.TransferCounterparty!.Id.Should().Be(income.Id);
        response.TransferCounterparty.Amount.Should().Be(91m);
        response.TransferCounterparty.Account.Id.Should().Be(toAccount.Id);
        response.TransferCounterparty.Account.Name.Should().Be("Euro Savings");
        response.TransferCounterparty.Account.CurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNotReturnTransferCounterparty_WhenPeerBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var otherAccount = TransactionTestData.CreateAccount(TransactionTestData.AnotherUserId, "EUR", "Other", "#222222");
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense, "#333333");
        var otherIncomeCategory = TransactionTestData.CreateCategory(TransactionTestData.AnotherUserId, "Transfer In", CategoryType.TransferIncome, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            fromAccount.Id,
            expenseCategory.Id,
            -100m,
            id: transactionId,
            account: fromAccount,
            category: expenseCategory,
            transferId: transferId);

        var otherIncome = TransactionTestData.CreateTransaction(
            TransactionTestData.AnotherUserId,
            otherAccount.Id,
            otherIncomeCategory.Id,
            91m,
            account: otherAccount,
            category: otherIncomeCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, otherIncome]);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TransferCounterparty.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNotReturnTransferCounterparty_WhenTransferPairUsesOperationCategories()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var toAccount = TransactionTestData.CreateAccount(userId, "EUR", "Euro Savings", "#222222");
        var debitCategory = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#333333");
        var creditCategory = TransactionTestData.CreateCategory(userId, "Salary", CategoryType.Credit, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            fromAccount.Id,
            debitCategory.Id,
            -100m,
            id: transactionId,
            account: fromAccount,
            category: debitCategory,
            transferId: transferId);

        var income = TransactionTestData.CreateTransaction(
            userId,
            toAccount.Id,
            creditCategory.Id,
            91m,
            account: toAccount,
            category: creditCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, income]);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TransferCounterparty.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNotReturnTransferCounterparty_WhenTransferPairUsesSameAccount()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var account = TransactionTestData.CreateAccount(userId, "USD", "Cash", "#111111");
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense, "#333333");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome, "#444444");
        var expense = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            expenseCategory.Id,
            -100m,
            id: transactionId,
            account: account,
            category: expenseCategory,
            transferId: transferId);

        var income = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            incomeCategory.Id,
            100m,
            account: account,
            category: incomeCategory,
            transferId: transferId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([expense, income]);

        var result = await sut.GetByIdAsync(transactionId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TransferCounterparty.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenUserWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserDomainErrors.UserNotFound);

        TransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenCategoryWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();
        var user = TransactionTestData.CreateUser(userId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.CategoryNotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenCategoryBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(TransactionTestData.AnotherUserId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.CategoryNotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenCategoryIsDeleted()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, isDeleted: true, id: request.CategoryId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.CategoryDeleted);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAmountIsZero()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest(amount: 0m);
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.AmountMustNotBeZero);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAmountSignDoesNotMatchDebitCategory()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest(amount: 10m);
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.AmountSignMismatchWithCategoryType);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAmountSignDoesNotMatchCreditCategory()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest(amount: -10m);
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Credit, id: request.CategoryId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.AmountSignMismatchWithCategoryType);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenCategoryIsTransferCategory()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest(amount: -10m);
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.TransferExpense, id: request.CategoryId);
        var account = TransactionTestData.CreateAccount(userId, id: request.AccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();

        TransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAccountBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);
        var account = TransactionTestData.CreateAccount(TransactionTestData.AnotherUserId, id: request.AccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAccountIsArchived()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest();
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);
        var account = TransactionTestData.CreateAccount(
            userId,
            isArchived: true,
            isPrimary: false,
            id: request.AccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        TransactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTransaction_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransactionRequest(amount: -15m, description: "  Coffee  ");
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);
        var account = TransactionTestData.CreateAccount(userId, id: request.AccountId);

        Transaction? createdTransaction = null;

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        TransactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((tx, _) => createdTransaction = tx)
            .Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        createdTransaction.Should().NotBeNull();
        createdTransaction!.UserId.Should().Be(userId);
        createdTransaction.AccountId.Should().Be(request.AccountId);
        createdTransaction.CategoryId.Should().Be(request.CategoryId);
        createdTransaction.Amount.Should().Be(-15m);
        createdTransaction.Description.Should().Be("Coffee");

        TransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTransactionWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateUpdateTransactionRequest();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransactionNotFound);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTransactionBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateUpdateTransactionRequest();
        var transaction = TransactionTestData.CreateTransaction(TransactionTestData.AnotherUserId, Guid.NewGuid(), Guid.NewGuid());

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransactionNotFound);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenValidationFails()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateUpdateTransactionRequest(amount: 0m);
        var transaction = TransactionTestData.CreateTransaction(userId, Guid.NewGuid(), Guid.NewGuid());
        var user = TransactionTestData.CreateUser(userId);
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit, id: request.CategoryId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.AmountMustNotBeZero);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTransactionBelongsToTransfer()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateUpdateTransactionRequest();
        var transaction = TransactionTestData.CreateTransaction(
            userId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            transferId: Guid.NewGuid());

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferTransactionRequiresTransferEndpoint);

        UserRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        TransactionRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTransactionAccountIsArchived()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var account = TransactionTestData.CreateAccount(
            userId,
            isArchived: true,
            isPrimary: false);
        var request = TransactionTestData.CreateUpdateTransactionRequest(amount: -25m);
        var transaction = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            Guid.NewGuid(),
            account: account);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        UserRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        TransactionRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTransaction_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.Debit);
        var request = TransactionTestData.CreateUpdateTransactionRequest(categoryId: category.Id, amount: -55m, description: "  Updated coffee  ");
        var transaction = TransactionTestData.CreateTransaction(userId, Guid.NewGuid(), Guid.NewGuid(), -10m, description: "Old");
        var user = TransactionTestData.CreateUser(userId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        TransactionRepositoryMock.Setup(x => x.UpdateAsync(transaction, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.UpdateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transaction.Id);
        transaction.CategoryId.Should().Be(request.CategoryId);
        transaction.Amount.Should().Be(-55m);
        transaction.Description.Should().Be("Updated coffee");

        TransactionRepositoryMock.Verify(x => x.UpdateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenNewCategoryIsTransferCategory()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var category = TransactionTestData.CreateCategory(userId, type: CategoryType.TransferIncome);
        var request = TransactionTestData.CreateUpdateTransactionRequest(categoryId: category.Id, amount: 55m);
        var transaction = TransactionTestData.CreateTransaction(userId, Guid.NewGuid(), Guid.NewGuid(), -10m, description: "Old");
        var user = TransactionTestData.CreateUser(userId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        transaction.CategoryId.Should().NotBe(request.CategoryId);

        TransactionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTransactionWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

        var result = await sut.DeleteAsync(transactionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransactionNotFound);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTransactionBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transaction = TransactionTestData.CreateTransaction(TransactionTestData.AnotherUserId, Guid.NewGuid(), Guid.NewGuid());

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);

        var result = await sut.DeleteAsync(transactionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransactionNotFound);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTransactionBelongsToTransfer()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transaction = TransactionTestData.CreateTransaction(
            userId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            id: transactionId,
            transferId: Guid.NewGuid());

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.DeleteAsync(transactionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferTransactionRequiresTransferEndpoint);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTransactionAccountIsArchived()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var account = TransactionTestData.CreateAccount(
            userId,
            isArchived: true,
            isPrimary: false);
        var transaction = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            Guid.NewGuid(),
            id: transactionId,
            account: account);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await sut.DeleteAsync(transactionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTransaction_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transactionId = Guid.NewGuid();
        var transaction = TransactionTestData.CreateTransaction(userId, Guid.NewGuid(), Guid.NewGuid(), id: transactionId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        TransactionRepositoryMock.Setup(x => x.RemoveAsync(transaction, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.DeleteAsync(transactionId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transaction.Id);

        TransactionRepositoryMock.Verify(x => x.RemoveAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferNotFound);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                TransactionTestData.AnotherUserId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferNotFound);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferPairIsIncomplete()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                amount: -100m,
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Transaction.TransferPairInvalid");

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferPairDoesNotHaveWithdrawalAndDeposit()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                amount: -100m,
                transferId: transferId),
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                amount: -50m,
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferPairInvalid);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferPairUsesOperationCategories()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var debitCategory = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var creditCategory = TransactionTestData.CreateCategory(userId, "Salary", CategoryType.Credit);
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                debitCategory.Id,
                amount: -100m,
                category: debitCategory,
                transferId: transferId),
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                creditCategory.Id,
                amount: 100m,
                category: creditCategory,
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferPairInvalid);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferAccountIsArchived()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, isArchived: true);
        var toAccount = TransactionTestData.CreateAccount(userId, name: "Savings");
        var transferExpenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);
        var transferIncomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome);
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                userId,
                fromAccount.Id,
                transferExpenseCategory.Id,
                amount: -100m,
                account: fromAccount,
                category: transferExpenseCategory,
                transferId: transferId),
            TransactionTestData.CreateTransaction(
                userId,
                toAccount.Id,
                transferIncomeCategory.Id,
                amount: 100m,
                account: toAccount,
                category: transferIncomeCategory,
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldReturnFailure_WhenTransferPairUsesSameAccount()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transferExpenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);
        var transferIncomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome);
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                userId,
                accountId,
                transferExpenseCategory.Id,
                amount: -100m,
                category: transferExpenseCategory,
                transferId: transferId),
            TransactionTestData.CreateTransaction(
                userId,
                accountId,
                transferIncomeCategory.Id,
                amount: 100m,
                category: transferIncomeCategory,
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferPairInvalid);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldRemoveAllTransferTransactionsAndSaveOnce()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var transferExpenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);
        var transferIncomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome);
        var transactions = new[]
        {
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                transferExpenseCategory.Id,
                amount: -100m,
                category: transferExpenseCategory,
                transferId: transferId),
            TransactionTestData.CreateTransaction(
                userId,
                Guid.NewGuid(),
                transferIncomeCategory.Id,
                amount: 100m,
                category: transferIncomeCategory,
                transferId: transferId)
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        TransactionRepositoryMock
            .Setup(x => x.GetByTransferIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        TransactionRepositoryMock
            .Setup(x => x.RemoveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.DeleteTransferAsync(transferId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transferId);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(transactions[0], It.IsAny<CancellationToken>()),
            Times.Once);

        TransactionRepositoryMock.Verify(
            x => x.RemoveAsync(transactions[1], It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTransferRateAsync_ShouldReturnExchangeRate_WhenAccountsBelongToCurrentUser()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: fromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: toAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(fromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(toAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        ExchangeRateServiceMock.Setup(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>())).ReturnsAsync(0.91m);

        var result = await sut.GetTransferRateAsync(fromAccountId, toAccountId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Rate.Should().Be(0.91m);
        result.Value.FromCurrencyCode.Should().Be("USD");
        result.Value.ToCurrencyCode.Should().Be("EUR");

        ExchangeRateServiceMock.Verify(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTransferRateAsync_ShouldReturnFailure_WhenExchangeRateIsNotPositive()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: fromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: toAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(fromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(toAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        ExchangeRateServiceMock.Setup(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>())).ReturnsAsync(-0.91m);

        var result = await sut.GetTransferRateAsync(fromAccountId, toAccountId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferRateMustBePositive);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenFromAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenFromAccountIsArchived()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest();
        var fromAccount = TransactionTestData.CreateAccount(userId, isArchived: true, isPrimary: false, id: request.FromAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenToAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest();
        var fromAccount = TransactionTestData.CreateAccount(userId, id: request.FromAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenToAccountIsArchived()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest();
        var fromAccount = TransactionTestData.CreateAccount(userId, id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, isArchived: true, isPrimary: false, id: request.ToAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenAccountsAreSame()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var accountId = Guid.NewGuid();
        var request = TransactionTestData.CreateTransferRequest(fromAccountId: accountId, toAccountId: accountId);
        var account = TransactionTestData.CreateAccount(userId, id: accountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferAccountsMustBeDifferent);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenAmountIsNotPositive()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(amount: 0m);
        var fromAccount = TransactionTestData.CreateAccount(userId, id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferAmountMustBeGreaterThanZero);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenCustomRateIsNotPositive()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(rate: 0m);
        var fromAccount = TransactionTestData.CreateAccount(userId, id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferRateMustBePositive);
        result.Error!.Details.Should().ContainSingle()
            .Which.Field.Should().Be("rate");
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenExchangeRateIsNotPositive()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(rate: null);
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Expense", CategoryType.TransferExpense, "#111111");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer Income", CategoryType.TransferIncome, "#222222");

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        ExchangeRateServiceMock.Setup(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>())).ReturnsAsync(-1.1m);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        CategoryRepositoryMock.Setup(x => x.GetTransferIncomeCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(incomeCategory);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionDomainErrors.TransferRateMustBePositive);

        TransactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenSameCurrencyCustomRateIsNotOne()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(rate: 1.5m);
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.ToAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Transaction.TransferRateMustBeOneForSameCurrency");
        result.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("rate");

        TransactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldUseRateOne_WhenCurrenciesAreSame()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(amount: 100m, rate: null);
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.ToAccountId);
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Expense", CategoryType.TransferExpense, "#111111");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer Income", CategoryType.TransferIncome, "#222222");

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        CategoryRepositoryMock.Setup(x => x.GetTransferIncomeCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(incomeCategory);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.TransferAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Rate.Should().Be(1m);
        result.Value.DepositAmount.Should().Be(100m);

        ExchangeRateServiceMock.Verify(x => x.GetRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldUseExchangeRateAndRoundDepositAmount_WhenCurrenciesDiffer()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(amount: 10.125m, rate: null);
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Expense", CategoryType.TransferExpense, "#111111");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer Income", CategoryType.TransferIncome, "#222222");

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        ExchangeRateServiceMock.Setup(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>())).ReturnsAsync(1.234m);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        CategoryRepositoryMock.Setup(x => x.GetTransferIncomeCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(incomeCategory);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.TransferAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Rate.Should().Be(1.234m);
        result.Value.DepositAmount.Should().Be(Math.Round(10.125m * 1.234m, 2, MidpointRounding.AwayFromZero));

        ExchangeRateServiceMock.Verify(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenRoundedDepositAmountIsZero()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(amount: 0.4m, rate: 1m);
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "JPY", id: request.ToAccountId);
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Expense", CategoryType.TransferExpense, "#111111");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer Income", CategoryType.TransferIncome, "#222222");

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        CategoryRepositoryMock.Setup(x => x.GetTransferIncomeCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(incomeCategory);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Transaction.TransferDepositAmountMustBeGreaterThanZero");
        result.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("amount");

        TransactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenTransferExpenseCategoryWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest();
        var fromAccount = TransactionTestData.CreateAccount(userId, id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        ExchangeRateServiceMock.Setup(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>())).ReturnsAsync(1.1m);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NotFound);
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnFailure_WhenTransferIncomeCategoryWasNotFound()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest();
        var fromAccount = TransactionTestData.CreateAccount(userId, id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Expense", CategoryType.TransferExpense, "#111111");

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        ExchangeRateServiceMock.Setup(x => x.GetRateAsync("USD", "EUR", It.IsAny<CancellationToken>())).ReturnsAsync(1.1m);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        CategoryRepositoryMock.Setup(x => x.GetTransferIncomeCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await sut.TransferAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NotFound);
    }

    [Fact]
    public async Task TransferAsync_ShouldCreateTwoTransactionsAndSaveOnce_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TransactionTestData.UserId;
        var request = TransactionTestData.CreateTransferRequest(amount: 100m, rate: 1.5m, description: "  To savings  ");
        var fromAccount = TransactionTestData.CreateAccount(userId, currencyCode: "USD", id: request.FromAccountId);
        var toAccount = TransactionTestData.CreateAccount(userId, currencyCode: "EUR", id: request.ToAccountId);
        var expenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Expense", CategoryType.TransferExpense, "#111111");
        var incomeCategory = TransactionTestData.CreateCategory(userId, "Transfer Income", CategoryType.TransferIncome, "#222222");

        var addedTransactions = new List<Transaction>();

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.FromAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(fromAccount);
        AccountRepositoryMock.Setup(x => x.GetByIdAsync(request.ToAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(toAccount);
        CategoryRepositoryMock.Setup(x => x.GetTransferExpenseCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        CategoryRepositoryMock.Setup(x => x.GetTransferIncomeCategoryAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(incomeCategory);
        TransactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((tx, _) => addedTransactions.Add(tx))
            .Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await sut.TransferAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        addedTransactions.Should().HaveCount(2);
        addedTransactions[0].AccountId.Should().Be(fromAccount.Id);
        addedTransactions[0].CategoryId.Should().Be(expenseCategory.Id);
        addedTransactions[0].Amount.Should().Be(-100m);
        addedTransactions[0].Description.Should().Be("To savings");
        addedTransactions[1].AccountId.Should().Be(toAccount.Id);
        addedTransactions[1].CategoryId.Should().Be(incomeCategory.Id);
        addedTransactions[1].Amount.Should().Be(150m);
        addedTransactions[1].Description.Should().Be("To savings");
        var expenseTransferId = addedTransactions[0].TransferId;
        var incomeTransferId = addedTransactions[1].TransferId;
        expenseTransferId.Should().NotBeNull();
        expenseTransferId.Should().NotBe(Guid.Empty);
        incomeTransferId.Should().Be(expenseTransferId);
        result.Value!.WithdrawAmount.Should().Be(100m);
        result.Value.DepositAmount.Should().Be(150m);
        result.Value.TransferId.Should().Be(expenseTransferId!.Value);

        TransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
