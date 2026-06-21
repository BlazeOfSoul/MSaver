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

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
