using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Entities;
using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Repositories;

public sealed class RepositoryTrackingTests
{
    [Fact]
    public async Task AccountGetByIdAsync_ShouldReturnTrackedEntityForWritePath()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var account = Account.Create(user.Id, "USD", "Main", "#111111");
        await context.Users.AddAsync(user);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetByIdAsync(account.Id);

        result.Should().NotBeNull();
        context.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task CategoryGetByIdAsync_ShouldReturnTrackedEntityForWritePath()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var category = Category.Create(user.Id, "Food", CategoryType.Debit, "#111111");
        await context.Users.AddAsync(user);
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new CategoryRepository(context);

        var result = await repository.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        context.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task TagGetByIdAsync_ShouldReturnTrackedEntityForWritePath()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var tag = Tag.Create(user.Id, "Home", "#111111");
        await context.Users.AddAsync(user);
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new TagRepository(context);

        var result = await repository.GetByIdAsync(tag.Id);

        result.Should().NotBeNull();
        context.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
