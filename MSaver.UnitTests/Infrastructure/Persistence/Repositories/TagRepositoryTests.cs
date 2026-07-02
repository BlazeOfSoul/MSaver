using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.UnitTests.Infrastructure.Persistence.Repositories;

public sealed class TagRepositoryTests
{
    [Fact]
    public async Task GetByIdWithCategoriesAsync_ShouldIncludeLinkedCategory()
    {
        var databaseName = Guid.NewGuid().ToString();
        var userId = TagTestData.UserId;
        var tag = TagTestData.CreateTag(userId, "Essentials", "#23c78b");
        var category = CategoryTestData.CreateCategory(
            userId,
            "Food",
            CategoryType.Debit,
            "#ff6f91");

        tag.ReplaceCategories([category.Id]);

        await using (var dbContext = CreateDbContext(databaseName))
        {
            dbContext.Tags.Add(tag);
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = CreateDbContext(databaseName);
        var repository = new TagRepository(readDbContext);

        var result = await repository.GetByIdWithCategoriesAsync(tag.Id);

        result.Should().NotBeNull();
        var tagCategory = result!.TagCategories.Should().ContainSingle().Subject;
        tagCategory.Category.Should().NotBeNull();
        tagCategory.Category!.Name.Should().Be("Food");
    }

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

    private static ApplicationDbContext CreateDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
