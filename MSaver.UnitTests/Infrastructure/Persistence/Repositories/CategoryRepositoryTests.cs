using Microsoft.EntityFrameworkCore;

using MSaver.Application.Features.Categories.Get;
using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldExcludeDeletedCategories()
    {
        await using var dbContext = CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var userId = CategoryTestData.UserId;
        var activeCategory = CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var deletedCategory = CategoryTestData.CreateCategory(userId, "Old Food", CategoryType.Debit);
        deletedCategory.SoftDelete();

        dbContext.Categories.AddRange(activeCategory, deletedCategory);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetPagedAsync(
            new CategoryListQuery
            {
                UserId = userId,
                Page = 1,
                Size = 20
            });

        result.Items.Should().ContainSingle().Subject.Id.Should().Be(activeCategory.Id);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldIgnoreDeletedCategories()
    {
        await using var dbContext = CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var userId = CategoryTestData.UserId;
        var deletedCategory = CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var activeCategory = CategoryTestData.CreateCategory(userId, "Salary", CategoryType.Credit);
        deletedCategory.SoftDelete();

        dbContext.Categories.AddRange(deletedCategory, activeCategory);
        await dbContext.SaveChangesAsync();

        var deletedNameExists = await repository.ExistsByNameAsync(userId, "Food");
        var activeNameExists = await repository.ExistsByNameAsync(userId, "Salary");

        deletedNameExists.Should().BeFalse();
        activeNameExists.Should().BeTrue();
    }

    [Fact]
    public async Task HasTransactionsAsync_ShouldReturnTrueOnlyWhenCategoryIsUsedByTransaction()
    {
        await using var dbContext = CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var userId = CategoryTestData.UserId;
        var account = TransactionTestData.CreateAccount(userId);
        var usedCategory = CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit);
        var unusedCategory = CategoryTestData.CreateCategory(userId, "Salary", CategoryType.Credit);
        var transaction = TransactionTestData.CreateTransaction(
            userId,
            account.Id,
            usedCategory.Id,
            account: account,
            category: usedCategory);

        dbContext.Accounts.Add(account);
        dbContext.Categories.AddRange(usedCategory, unusedCategory);
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        var usedCategoryHasTransactions = await repository.HasTransactionsAsync(usedCategory.Id);
        var unusedCategoryHasTransactions = await repository.HasTransactionsAsync(unusedCategory.Id);

        usedCategoryHasTransactions.Should().BeTrue();
        unusedCategoryHasTransactions.Should().BeFalse();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
