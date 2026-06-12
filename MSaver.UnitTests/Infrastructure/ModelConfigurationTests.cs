using Microsoft.EntityFrameworkCore;

using MSaver.Infrastructure;

namespace MSaver.UnitTests.Infrastructure;

public sealed class ModelConfigurationTests
{
    [Fact]
    public void AccountNames_ShouldBeUniquePerUserForActiveAccounts()
    {
        using var context = CreateContext();

        var index = context.Model
            .FindEntityType(typeof(Account))!
            .GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(Account.UserId), nameof(Account.Name)]));

        index.IsUnique.Should().BeTrue();
        index.GetFilter().Should().Contain(nameof(Account.IsArchived));
    }

    [Fact]
    public void CategoryNames_ShouldBeUniquePerUser()
    {
        using var context = CreateContext();

        var index = context.Model
            .FindEntityType(typeof(Category))!
            .GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(Category.UserId), nameof(Category.Name)]));

        index.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void TagNames_ShouldBeUniquePerUser()
    {
        using var context = CreateContext();

        var index = context.Model
            .FindEntityType(typeof(Tag))!
            .GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(Tag.UserId), nameof(Tag.Name)]));

        index.IsUnique.Should().BeTrue();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
