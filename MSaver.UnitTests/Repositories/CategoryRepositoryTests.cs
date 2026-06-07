using Microsoft.EntityFrameworkCore;

using MSaver.Application.Features.Categories.Get;
using MSaver.Domain.Entities;
using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Repositories;

public sealed class CategoryRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldNotReturnDeletedCategories()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var activeCategory = Category.Create(user.Id, "Food", CategoryType.Debit, "#111111");
        var deletedCategory = Category.Create(user.Id, "Old Food", CategoryType.Debit, "#222222");
        deletedCategory.SoftDelete();

        await context.Users.AddAsync(user);
        await context.Categories.AddRangeAsync(activeCategory, deletedCategory);
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);

        var result = await repository.GetPagedAsync(new CategoryListQuery
        {
            UserId = user.Id,
            Page = 1,
            Size = 100
        });

        result.Items.Should().ContainSingle();
        result.Items.Single().Id.Should().Be(activeCategory.Id);
        result.TotalCount.Should().Be(1);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
