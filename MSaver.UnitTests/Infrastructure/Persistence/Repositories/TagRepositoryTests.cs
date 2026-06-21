using Microsoft.EntityFrameworkCore;

using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Infrastructure.Persistence.Repositories;

public sealed class TagRepositoryTests
{
    [Fact]
    public async Task ExistsByNameAsync_ShouldIgnoreDeletedTags()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TagRepository(dbContext);
        var userId = TagTestData.UserId;
        var deletedTag = TagTestData.CreateTag(userId, "Important");
        var activeTag = TagTestData.CreateTag(userId, "Recurring");
        deletedTag.Delete();

        dbContext.Tags.AddRange(deletedTag, activeTag);
        await dbContext.SaveChangesAsync();

        var deletedNameExists = await repository.ExistsByNameAsync(userId, "Important");
        var activeNameExists = await repository.ExistsByNameAsync(userId, "Recurring");

        deletedNameExists.Should().BeFalse();
        activeNameExists.Should().BeTrue();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
