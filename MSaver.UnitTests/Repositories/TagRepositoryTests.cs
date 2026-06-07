using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Entities;
using MSaver.Domain.Enums;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Persistence;
using MSaver.Infrastructure.Persistence.Repositories;
using MSaver.UnitTests.Common.TestData;

namespace MSaver.UnitTests.Repositories;

public sealed class TagRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldNotLoadTagCategories()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var category = Category.Create(user.Id, "Food", CategoryType.Debit, "#111111");
        var tag = Tag.Create(user.Id, "Home", "#23c78b");
        tag.ReplaceCategories([category.Id]);

        await context.Users.AddAsync(user);
        await context.Categories.AddAsync(category);
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new TagRepository(context);

        var result = await repository.GetByIdAsync(tag.Id);

        result.Should().NotBeNull();
        result!.TagCategories.Should().BeEmpty();
        context.ChangeTracker.Entries<TagCategory>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdWithCategoriesAsync_ShouldLoadAssignedCategoryDetails()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var category = Category.Create(user.Id, "Food", CategoryType.Debit, "#111111");
        var tag = Tag.Create(user.Id, "Home", "#23c78b");
        tag.ReplaceCategories([category.Id]);

        await context.Users.AddAsync(user);
        await context.Categories.AddAsync(category);
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new TagRepository(context);

        var result = await repository.GetByIdWithCategoriesAsync(tag.Id);

        result.Should().NotBeNull();
        result!.TagCategories.Should().ContainSingle();
        result.TagCategories.Single().Category.Should().NotBeNull();
        result.TagCategories.Single().Category!.Name.Should().Be("Food");
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldPersistAssignedCategoryDetails()
    {
        await using var context = CreateContext();
        var user = User.Create("Rostik", "rostik@example.com", "hash");
        var category = Category.Create(user.Id, "Food", CategoryType.Debit, "#111111");
        var tag = Tag.Create(user.Id, "Home", "#23c78b");

        await context.Users.AddAsync(user);
        await context.Categories.AddAsync(category);
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService
            .Setup(x => x.UserId)
            .Returns(user.Id);

        var tagRepository = new TagRepository(context);
        var categoryRepository = new CategoryRepository(context);
        var service = new TagService(
            new UserRepository(context),
            tagRepository,
            categoryRepository,
            new UnitOfWork(context),
            currentUserService.Object);

        var result = await service.AssignCategoriesAsync(
            TagTestData.CreateAssignCategoriesRequest(tag.Id, [category.Id]));

        context.ChangeTracker.Clear();
        var persisted = await tagRepository.GetByIdWithCategoriesAsync(tag.Id);

        result.IsSuccess.Should().BeTrue();
        persisted.Should().NotBeNull();
        persisted!.TagCategories.Should().ContainSingle();
        persisted.TagCategories.Single().Category.Should().NotBeNull();
        persisted.TagCategories.Single().Category!.Name.Should().Be("Food");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
