using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepositoryTests
{
    [Fact]
    public async Task SumByAccountIdsAsync_ShouldSumSignedTransactionAmounts()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var accountId = Guid.NewGuid();
        var debitCategory = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var transferExpenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);
        var creditCategory = TransactionTestData.CreateCategory(userId, "Salary", CategoryType.Credit);

        dbContext.Categories.AddRange(debitCategory, transferExpenseCategory, creditCategory);
        dbContext.Transactions.AddRange(
            TransactionTestData.CreateTransaction(userId, accountId, debitCategory.Id, -25m, category: debitCategory),
            TransactionTestData.CreateTransaction(userId, accountId, transferExpenseCategory.Id, -100m, category: transferExpenseCategory),
            TransactionTestData.CreateTransaction(userId, accountId, creditCategory.Id, 300m, category: creditCategory));
        await dbContext.SaveChangesAsync();

        var totals = await repository.SumByAccountIdsAsync([accountId]);

        totals.Should().ContainKey(accountId);
        totals[accountId].Should().Be(175m);
    }

    [Fact]
    public async Task GetBalanceBeforeAsync_ShouldSumSignedAmountsBeforeExclusiveDate()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var accountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        var category = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var cutoff = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        dbContext.Categories.Add(category);
        dbContext.Transactions.AddRange(
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, -100m, cutoff.AddDays(-2), category: category),
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, 250m, cutoff.AddTicks(-1), category: category),
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, 999m, cutoff, category: category),
            TransactionTestData.CreateTransaction(userId, otherAccountId, category.Id, -999m, cutoff.AddDays(-1), category: category));
        await dbContext.SaveChangesAsync();

        var balance = await repository.GetBalanceBeforeAsync(accountId, cutoff);

        balance.Should().Be(150m);
    }

    [Fact]
    public async Task GetBalanceInPeriodAsync_ShouldUseInclusiveStartAndExclusiveEnd()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var accountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        var category = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var periodStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        dbContext.Categories.Add(category);
        dbContext.Transactions.AddRange(
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, -999m, periodStart.AddTicks(-1), category: category),
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, -100m, periodStart, category: category),
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, 50m, periodStart.AddDays(10), category: category),
            TransactionTestData.CreateTransaction(userId, accountId, category.Id, 999m, periodEnd, category: category),
            TransactionTestData.CreateTransaction(userId, otherAccountId, category.Id, 999m, periodStart.AddDays(10), category: category));
        await dbContext.SaveChangesAsync();

        var balance = await repository.GetBalanceInPeriodAsync(
            accountId,
            periodStart,
            periodEnd);

        balance.Should().Be(-50m);
    }

    [Fact]
    public async Task GetBreakdownInPeriodAsync_ShouldSplitOperationsAndTransfersByCategoryType()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var accountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        var debitCategory = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var creditCategory = TransactionTestData.CreateCategory(userId, "Salary", CategoryType.Credit);
        var transferExpenseCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);
        var transferIncomeCategory = TransactionTestData.CreateCategory(userId, "Transfer In", CategoryType.TransferIncome);
        var periodStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        dbContext.Categories.AddRange(debitCategory, creditCategory, transferExpenseCategory, transferIncomeCategory);
        dbContext.Transactions.AddRange(
            TransactionTestData.CreateTransaction(userId, accountId, debitCategory.Id, -25m, periodStart.AddDays(1), category: debitCategory),
            TransactionTestData.CreateTransaction(userId, accountId, creditCategory.Id, 300m, periodStart.AddDays(2), category: creditCategory),
            TransactionTestData.CreateTransaction(userId, accountId, transferExpenseCategory.Id, -100m, periodStart.AddDays(3), category: transferExpenseCategory),
            TransactionTestData.CreateTransaction(userId, accountId, transferIncomeCategory.Id, 40m, periodStart.AddDays(4), category: transferIncomeCategory),
            TransactionTestData.CreateTransaction(userId, accountId, debitCategory.Id, -999m, periodStart.AddDays(-1), category: debitCategory),
            TransactionTestData.CreateTransaction(userId, accountId, creditCategory.Id, 999m, periodEnd, category: creditCategory),
            TransactionTestData.CreateTransaction(userId, otherAccountId, creditCategory.Id, 999m, periodStart.AddDays(5), category: creditCategory));
        await dbContext.SaveChangesAsync();

        var breakdown = await repository.GetBreakdownInPeriodAsync(
            accountId,
            periodStart,
            periodEnd);

        breakdown.Income.Should().Be(300m);
        breakdown.Expense.Should().Be(25m);
        breakdown.TransferIn.Should().Be(40m);
        breakdown.TransferOut.Should().Be(100m);
        breakdown.OperationsChange.Should().Be(275m);
        breakdown.TransferChange.Should().Be(-60m);
    }

    [Fact]
    public async Task GetByTransferIdAsync_ShouldReturnOnlyTransactionsWithMatchingTransferId()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var account = TransactionTestData.CreateAccount(userId, "USD", "Cash");
        var otherAccount = TransactionTestData.CreateAccount(userId, "USD", "Savings");
        var transferCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);
        var transferId = Guid.NewGuid();
        var otherTransferId = Guid.NewGuid();

        var matchingExpense = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            transferCategory.Id,
            -100m,
            account: account,
            category: transferCategory,
            transferId: transferId);

        var matchingIncome = TransactionTestData.CreateTransaction(
            userId,
            otherAccount.Id,
            transferCategory.Id,
            100m,
            account: otherAccount,
            category: transferCategory,
            transferId: transferId);

        var otherTransfer = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            transferCategory.Id,
            -25m,
            account: account,
            category: transferCategory,
            transferId: otherTransferId);

        dbContext.Accounts.AddRange(account, otherAccount);
        dbContext.Categories.Add(transferCategory);
        dbContext.Transactions.AddRange(matchingExpense, matchingIncome, otherTransfer);
        await dbContext.SaveChangesAsync();

        var transactions = await repository.GetByTransferIdAsync(transferId);

        transactions.Should().HaveCount(2);
        transactions.Select(x => x.Id).Should().BeEquivalentTo([matchingExpense.Id, matchingIncome.Id]);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTransactionWithAccountDetails()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var account = TransactionTestData.CreateAccount(userId, "USD", "Cash");
        var category = TransactionTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var transaction = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            category.Id,
            -25m,
            account: null,
            category: category);

        dbContext.Accounts.Add(account);
        dbContext.Categories.Add(category);
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(transaction.Id);

        result.Should().NotBeNull();
        result!.Account.Should().NotBeNull();
        result.Account!.Id.Should().Be(account.Id);
    }

    [Fact]
    public async Task GetByTransferIdsWithDetailsAsync_ShouldReturnMatchingTransfersWithAccountDetails()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TransactionRepository(dbContext);
        var userId = TransactionTestData.UserId;
        var transferId = Guid.NewGuid();
        var otherTransferId = Guid.NewGuid();
        var account = TransactionTestData.CreateAccount(userId, "USD", "Cash");
        var otherAccount = TransactionTestData.CreateAccount(userId, "EUR", "Euro Savings");
        var transferCategory = TransactionTestData.CreateCategory(userId, "Transfer Out", CategoryType.TransferExpense);

        var matchingExpense = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            transferCategory.Id,
            -100m,
            account: account,
            category: transferCategory,
            transferId: transferId);

        var matchingIncome = TransactionTestData.CreateTransaction(
            userId,
            otherAccount.Id,
            transferCategory.Id,
            91m,
            account: otherAccount,
            category: transferCategory,
            transferId: transferId);

        var otherTransfer = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            transferCategory.Id,
            -25m,
            account: account,
            category: transferCategory,
            transferId: otherTransferId);

        dbContext.Accounts.AddRange(account, otherAccount);
        dbContext.Categories.Add(transferCategory);
        dbContext.Transactions.AddRange(matchingExpense, matchingIncome, otherTransfer);
        await dbContext.SaveChangesAsync();

        var transactions = await repository.GetByTransferIdsWithDetailsAsync([transferId]);

        transactions.Should().HaveCount(2);
        transactions.Select(x => x.Id).Should().BeEquivalentTo([matchingExpense.Id, matchingIncome.Id]);
        transactions.Should().OnlyContain(x => x.Account != null);
        transactions.Select(x => x.Account!.Name).Should().BeEquivalentTo(["Cash", "Euro Savings"]);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
