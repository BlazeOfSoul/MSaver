using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Repositories;

public sealed class TransactionRepositoryTests
{
    [Fact]
    public async Task GetBalanceForPeriodAsync_ShouldReturnOpeningBalanceAndPeriodChange()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var account = Account.Create(user.Id, "USD", "Cash");
        var category = Category.Create(user.Id, "Food", CategoryType.Debit, "#111111");
        var monthStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        await context.Users.AddAsync(user);
        await context.Accounts.AddAsync(account);
        await context.Categories.AddAsync(category);
        await context.Transactions.AddRangeAsync(
            Transaction.Create(user.Id, account.Id, category.Id, 100m, monthStart.AddDays(-1), "before"),
            Transaction.Create(user.Id, account.Id, category.Id, -25m, monthStart.AddDays(2), "inside"),
            Transaction.Create(user.Id, account.Id, category.Id, 999m, monthEnd, "after"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new TransactionRepository(context);

        var result = await repository.GetBalanceForPeriodAsync(
            account.Id,
            monthStart,
            monthEnd);

        result.OpeningBalance.Should().Be(100m);
        result.PeriodChange.Should().Be(-25m);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
