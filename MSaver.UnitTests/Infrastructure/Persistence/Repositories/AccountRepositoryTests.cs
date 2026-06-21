using Microsoft.EntityFrameworkCore;

using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Infrastructure.Persistence.Repositories;

public sealed class AccountRepositoryTests
{
    [Fact]
    public async Task ExistsByNameAsync_ShouldIgnoreArchivedAccounts()
    {
        await using var dbContext = CreateDbContext();
        var repository = new AccountRepository(dbContext);
        var userId = AccountTestData.UserId;
        var archivedAccount = AccountTestData.CreateAccount(userId: userId, name: "Cash");
        var activeAccount = AccountTestData.CreateAccount(userId: userId, name: "Savings");
        archivedAccount.Archive();

        dbContext.Accounts.AddRange(archivedAccount, activeAccount);
        await dbContext.SaveChangesAsync();

        var archivedNameExists = await repository.ExistsByNameAsync(userId, "Cash");
        var activeNameExists = await repository.ExistsByNameAsync(userId, "Savings");

        archivedNameExists.Should().BeFalse();
        activeNameExists.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_ShouldIgnoreArchivedAccounts()
    {
        await using var dbContext = CreateDbContext();
        var repository = new AccountRepository(dbContext);
        var userId = AccountTestData.UserId;
        var archivedAccount = AccountTestData.CreateAccount(userId: userId, name: "Old Cash");
        archivedAccount.Archive();

        dbContext.Accounts.Add(archivedAccount);
        await dbContext.SaveChangesAsync();

        var hasActiveAccounts = await repository.AnyAsync(userId);

        hasActiveAccounts.Should().BeFalse();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
