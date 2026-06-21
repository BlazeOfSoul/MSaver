using MSaver.Api.Contracts.Accounts;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Domain.Common;
using MSaver.UnitTests.Common;
using MSaver.UnitTests.Common.TestData;

namespace MSaver.UnitTests.Services;

public sealed class AccountServiceTests : AccountServiceTestBase
{
    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenAccountNameAlreadyExists()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.CreateAccountRequest(name: "Main account");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(true);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NameAlreadyExists);

        AccountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePrimaryAccount_WhenUserDoesNotHaveAnyAccounts()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.CreateAccountRequest(
            name: "Main account",
            currencyCode: "usd",
            color: "#123456",
            initialBalance: 250.50m);

        Account? createdAccount = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(false);

        AccountRepositoryMock
            .Setup(x => x.AnyAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AccountRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Callback<Account, CancellationToken>((account, _) => createdAccount = account)
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        createdAccount.Should().NotBeNull();
        createdAccount!.UserId.Should().Be(userId);
        createdAccount.Name.Should().Be("Main account");
        createdAccount.CurrencyCode.Should().Be("USD");
        createdAccount.Color.Should().Be("#123456");
        createdAccount.InitialBalance.Should().Be(250.50m);
        createdAccount.IsPrimary.Should().BeTrue();
        createdAccount.IsArchived.Should().BeFalse();

        AccountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenInitialBalanceIsNegative()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.CreateAccountRequest(initialBalance: -0.01m);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.InitialBalanceNegative);

        AccountRepositoryMock.Verify(
            x => x.ExistsByNameAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Guid?>()),
            Times.Never);

        AccountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNonPrimaryAccount_WhenUserAlreadyHasAccounts()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.CreateAccountRequest(
            name: "Savings",
            currencyCode: "eur",
            color: "#654321");

        Account? createdAccount = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(false);

        AccountRepositoryMock
            .Setup(x => x.AnyAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        AccountRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Callback<Account, CancellationToken>((account, _) => createdAccount = account)
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        createdAccount.Should().NotBeNull();
        createdAccount!.IsPrimary.Should().BeFalse();
        createdAccount.CurrencyCode.Should().Be("EUR");

        AccountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.UpdateAccountRequest(
            id: Guid.NewGuid(),
            name: "Updated name",
            color: "#ABC123");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenAccountBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: Guid.NewGuid(), name: "Foreign");
        var request = RequestFactory.UpdateAccountRequest(
            id: account.Id,
            name: "Updated name",
            color: "#ABC123");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        AccountRepositoryMock.Verify(
            x => x.ExistsByNameAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Guid?>()),
            Times.Never);

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenAccountNameAlreadyExists()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: userId, name: "Old name", color: "#000000");
        var request = RequestFactory.UpdateAccountRequest(
            id: account.Id,
            name: "Updated name",
            color: "#ABC123");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        AccountRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                request.Id))
            .ReturnsAsync(true);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NameAlreadyExists);

        account.Name.Should().Be("Old name");
        account.Color.Should().Be("#000000");

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAccount_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: userId, name: "Old name", color: "#000000");
        var request = RequestFactory.UpdateAccountRequest(
            id: account.Id,
            name: "Updated name",
            color: "#ABC123");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        AccountRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                request.Id))
            .ReturnsAsync(false);

        AccountRepositoryMock
            .Setup(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.UpdateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(account.Id);

        account.Name.Should().Be("Updated name");
        account.Color.Should().Be("#ABC123");

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var accountId = Guid.NewGuid();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await sut.DeleteAsync(accountId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenAccountBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: Guid.NewGuid(), name: "Foreign", isPrimary: false);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await sut.DeleteAsync(account.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldArchiveAccount_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: userId, name: "Archive me", isPrimary: false);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        AccountRepositoryMock
            .Setup(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.DeleteAsync(account.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(account.Id);
        account.IsArchived.Should().BeTrue();

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenAccountIsPrimary()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: userId, name: "Primary", isPrimary: true);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        await Assert.ThrowsAsync<DomainException>(() => sut.DeleteAsync(account.Id));

        AccountRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldNormalizeQueryFields()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;

        var request = new GetAccountsRequest
        {
            Search = "  cash  ",
            SortBy = "  name  ",
            SortDirection = "ASC",
            IsArchived = true,
            CurrencyCode = " usd ",
            Page = 3,
            Size = 20
        };

        AccountListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<AccountListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<AccountListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(new PagedResult<Account>
            {
                Items = [],
                Page = 3,
                Size = 20,
                TotalCount = 0
            });

        TransactionRepositoryMock
            .Setup(x => x.SumByAccountIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await sut.GetAccountsAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Search.Should().Be("cash");
        capturedQuery.SortBy.Should().Be("name");
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortAscending);
        capturedQuery.IsArchived.Should().BeTrue();
        capturedQuery.CurrencyCode.Should().Be("USD");
        capturedQuery.Page.Should().Be(3);
        capturedQuery.Size.Should().Be(20);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldReturnPagedAccountsWithBalances()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;

        var account1 = AccountTestData.CreateAccount(
            userId: userId,
            name: "Cash",
            color: "#111111",
            initialBalance: 100m);

        var account2 = AccountTestData.CreateAccount(
            userId: userId,
            name: "Card",
            color: "#222222",
            initialBalance: 20m);

        var pagedAccounts = new PagedResult<Account>
        {
            Items = new[] { account1, account2 },
            Page = 2,
            Size = 10,
            TotalCount = 25
        };

        var totals = new Dictionary<Guid, decimal>
        {
            [account1.Id] = 150.75m,
            [account2.Id] = -20m
        };

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<AccountListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedAccounts);

        TransactionRepositoryMock
            .Setup(x => x.SumByAccountIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var result = await sut.GetAccountsAsync(new GetAccountsRequest
        {
            Page = 2,
            Size = 10
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Items.Should().HaveCount(2);
        response.Page.Should().Be(2);
        response.Size.Should().Be(10);
        response.TotalCount.Should().Be(25);
        response.TotalPages.Should().Be(3);
        response.HasPreviousPage.Should().BeTrue();
        response.HasNextPage.Should().BeTrue();

        var items = response.Items.ToArray();

        items[0].Id.Should().Be(account1.Id);
        items[0].Name.Should().Be("Cash");
        items[0].CurrencyCode.Should().Be(account1.CurrencyCode);
        items[0].InitialBalance.Should().Be(100m);
        items[0].CurrentBalance.Should().Be(250.75m);
        items[0].Color.Should().Be("#111111");
        items[0].IsPrimary.Should().Be(account1.IsPrimary);

        items[1].Id.Should().Be(account2.Id);
        items[1].InitialBalance.Should().Be(20m);
        items[1].CurrentBalance.Should().Be(0m);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldUseZeroBalance_WhenTotalsDoNotContainAccount()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: userId, name: "Cash");

        var pagedAccounts = new PagedResult<Account>
        {
            Items = new[] { account },
            Page = 1,
            Size = 10,
            TotalCount = 1
        };

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<AccountListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedAccounts);

        TransactionRepositoryMock
            .Setup(x => x.SumByAccountIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await sut.GetAccountsAsync(new GetAccountsRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var singleItem = result.Value!.Items
            .Should()
            .ContainSingle()
            .Subject;

        singleItem.InitialBalance.Should().Be(0m);
        singleItem.CurrentBalance.Should().Be(0m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var accountId = Guid.NewGuid();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await sut.GetByIdAsync(accountId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenAccountBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: Guid.NewGuid(), isPrimary: false);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await sut.GetByIdAsync(account.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAccountWithZeroBalance_WhenTotalsDoNotContainAccount()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var account = AccountTestData.CreateAccount(userId: userId, isPrimary: true, initialBalance: 50m);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        TransactionRepositoryMock
            .Setup(x => x.SumByAccountIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await sut.GetByIdAsync(account.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InitialBalance.Should().Be(50m);
        result.Value.CurrentBalance.Should().Be(50m);
        result.Value.Name.Should().Be(account.Name);
        result.Value.CurrencyCode.Should().Be(account.CurrencyCode);
        result.Value.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAccountWithBalance_WhenTotalsContainAccount()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var createdAt = new DateTime(2026, 4, 12, 9, 30, 0, DateTimeKind.Utc);
        var account = AccountTestData.CreateAccount(
            userId: userId,
            isPrimary: false,
            initialBalance: 200m,
            createdAt: createdAt);

        var totals = new Dictionary<Guid, decimal>
        {
            [account.Id] = 999.99m
        };

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        TransactionRepositoryMock
            .Setup(x => x.SumByAccountIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var result = await sut.GetByIdAsync(account.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InitialBalance.Should().Be(200m);
        result.Value.CurrentBalance.Should().Be(1199.99m);
        result.Value.IsPrimary.Should().BeFalse();
        result.Value.CreatedAtUtc.Should().Be(createdAt);
    }

    [Fact]
    public async Task GetMonthBalanceAsync_ShouldReturnFailure_WhenAccountWasNotFound()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.GetMonthBalanceRequest(Guid.NewGuid(), 2026, 5);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await sut.GetMonthBalanceAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task GetMonthBalanceAsync_ShouldReturnFailure_WhenAccountBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var request = RequestFactory.GetMonthBalanceRequest(Guid.NewGuid(), 2026, 5);
        var account = AccountTestData.CreateAccount(userId: Guid.NewGuid(), isPrimary: false);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await sut.GetMonthBalanceAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountDomainErrors.NotFound);
    }

    [Fact]
    public async Task GetMonthBalanceAsync_ShouldReturnCalculatedBalances_WhenAccountExists()
    {
        var sut = CreateSut();
        var userId = AccountTestData.UserId;
        var accountId = Guid.NewGuid();
        var request = RequestFactory.GetMonthBalanceRequest(accountId, 2026, 5);
        var account = AccountTestData.CreateAccount(
            userId: userId,
            name: "Main account",
            color: "#999999",
            initialBalance: 1000m);

        var expectedMonthStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var expectedMonthEnd = expectedMonthStart.AddMonths(1);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        AccountRepositoryMock
            .Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        TransactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeAsync(
                account.Id,
                expectedMonthStart,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(100m);

        TransactionRepositoryMock
            .Setup(x => x.GetBalanceInPeriodAsync(
                account.Id,
                expectedMonthStart,
                expectedMonthEnd,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(25m);

        TransactionRepositoryMock
            .Setup(x => x.GetBreakdownInPeriodAsync(
                account.Id,
                expectedMonthStart,
                expectedMonthEnd,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransactionPeriodBreakdown(
                Income: 300m,
                Expense: -120m,
                TransferIn: 50m,
                TransferOut: -205m));

        var result = await sut.GetMonthBalanceAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.AccountId.Should().Be(account.Id);
        result.Value.AccountName.Should().Be(account.Name);
        result.Value.CurrencyCode.Should().Be(account.CurrencyCode);
        result.Value.OpeningBalance.Should().Be(1100m);
        result.Value.MonthChange.Should().Be(25m);
        result.Value.ClosingBalance.Should().Be(1125m);
        result.Value.Income.Should().Be(300m);
        result.Value.Expense.Should().Be(-120m);
        result.Value.TransferIn.Should().Be(50m);
        result.Value.TransferOut.Should().Be(-205m);
        result.Value.OperationsChange.Should().Be(180m);
        result.Value.TransferChange.Should().Be(-155m);
        result.Value.Year.Should().Be(2026);
        result.Value.Month.Should().Be(5);
    }
}
