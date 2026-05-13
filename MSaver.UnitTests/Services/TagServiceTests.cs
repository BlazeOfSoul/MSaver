using MSaver.Api.Contracts.Tags;
using MSaver.Application.Features.Tags.Get;
using MSaver.Domain.Enums;
using MSaver.UnitTests.Common;
using MSaver.UnitTests.Common.TestData;

namespace MSaver.UnitTests.Services;

public sealed class TagServiceTests : TagServiceTestBase
{
    [Fact]
    public async Task GetAsync_ShouldReturnMappedPagedTags_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;

        var request = TagTestData.CreateGetTagsRequest(
            search: " groceries ",
            sortBy: TagSortFields.Name,
            sortDirection: ListQueryDefaults.SortAscending,
            page: 2,
            size: 5);

        var tags = new[]
        {
            TagTestData.CreateTag(userId, "Groceries", "#FF0000"),
            TagTestData.CreateTag(userId, "Salary", "#00FF00")
        };

        var pagedTags = TagTestData.CreatePagedTags(tags, page: 2, size: 5, totalCount: 12);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<TagListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedTags);

        var result = await sut.GetAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Items.Should().HaveCount(2);
        response.Page.Should().Be(2);
        response.Size.Should().Be(5);
        response.TotalCount.Should().Be(12);
        response.TotalPages.Should().Be(pagedTags.TotalPages);
        response.HasPreviousPage.Should().BeTrue();
        response.HasNextPage.Should().BeTrue();

        response.Items.First().Name.Should().Be("Groceries");
        response.Items.First().Color.Should().Be("#FF0000");
        response.Items.Last().Name.Should().Be("Salary");
        response.Items.Last().Color.Should().Be("#00FF00");
    }

    [Fact]
    public async Task GetAsync_ShouldNormalizeQueryFields_WhenBuildingQuery()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;

        var request = new GetTagsRequest
        {
            Search = "  groceries  ",
            SortBy = "  name  ",
            SortDirection = "ASC",
            Page = 3,
            Size = 15
        };

        TagListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<TagListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TagListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(TagTestData.CreatePagedTags([], page: 3, size: 15, totalCount: 0));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Search.Should().Be("groceries");
        capturedQuery.SortBy.Should().Be("name");
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortAscending);
        capturedQuery.Page.Should().Be(3);
        capturedQuery.Size.Should().Be(15);
    }

    [Fact]
    public async Task GetAsync_ShouldUseDefaultPagingAndSorting_WhenRequestHasDefaultValues()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = new GetTagsRequest();

        TagListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<TagListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TagListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(TagTestData.CreatePagedTags([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Search.Should().BeNull();
        capturedQuery.SortBy.Should().Be(TagSortFields.Name);
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortAscending);
        capturedQuery.Page.Should().Be(ListQueryDefaults.DefaultPage);
        capturedQuery.Size.Should().Be(ListQueryDefaults.DefaultPageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenTagWasNotFound()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tagId = Guid.NewGuid();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await sut.GetByIdAsync(tagId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenTagBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = TagTestData.UserId;
        var tagId = Guid.NewGuid();
        var tag = TagTestData.CreateTag(TagTestData.AnotherUserId, "Groceries", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.GetByIdAsync(tagId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.AccessDenied);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedTagWithSortedCategories_WhenTagExists()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tagId = Guid.NewGuid();

        var food = CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#111111");
        var auto = CategoryTestData.CreateCategory(userId, "Auto", CategoryType.Credit, "#222222");
        var deleted = CategoryTestData.CreateCategory(userId, "Deleted Category", CategoryType.Debit, "#333333");
        deleted.SoftDelete();

        var tag = TagTestData.CreateTagWithCategories(
            userId,
            "Important",
            "#ABCDEF",
            auto,
            food,
            deleted,
            includeNullCategoryLink: true);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.GetByIdAsync(tagId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Name.Should().Be(tag.Name);
        response.Color.Should().Be(tag.Color);
        response.IsDeleted.Should().BeFalse();
        response.Categories.Should().HaveCount(3);

        response.Categories.Select(x => x.Name)
            .Should()
            .ContainInOrder("Auto", "Deleted Category", "Food");

        response.Categories.First(x => x.Name == "Auto").Type.Should().Be(CategoryType.Credit.ToString());
        response.Categories.First(x => x.Name == "Deleted Category").IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenUserWasNotFound()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateTagRequest();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserDomainErrors.UserNotFound);

        TagRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenTagNameAlreadyExists()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateTagRequest(name: "Important");
        var user = TagTestData.CreateUser(userId);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        TagRepositoryMock
            .Setup(x => x.ExistsByNameAsync(userId, request.Name, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(true);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.NameAlreadyExists);

        TagRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTag_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateTagRequest(name: "  Important  ", color: "  #FF0000  ");
        var user = TagTestData.CreateUser(userId);

        Tag? createdTag = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        TagRepositoryMock
            .Setup(x => x.ExistsByNameAsync(userId, request.Name, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(false);

        TagRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .Callback<Tag, CancellationToken>((tag, _) => createdTag = tag)
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        createdTag.Should().NotBeNull();
        createdTag!.UserId.Should().Be(userId);
        createdTag.Name.Should().Be("Important");
        createdTag.Color.Should().Be("#FF0000");
        createdTag.IsDeleted.Should().BeFalse();

        TagRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTagWasNotFound()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateUpdateTagRequest();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagNotFound);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTagBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var request = TagTestData.CreateUpdateTagRequest();
        var currentUserId = TagTestData.UserId;
        var tag = TagTestData.CreateTag(TagTestData.AnotherUserId, "Important", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.AccessDenied);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenTagIsDeleted()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateUpdateTagRequest();
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");
        tag.Delete();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagDeleted);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenAnotherTagWithSameNameExists()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateUpdateTagRequest(name: "Important");
        var tag = TagTestData.CreateTag(userId, "Old Name", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        TagRepositoryMock
            .Setup(x => x.ExistsByNameAsync(userId, request.Name, It.IsAny<CancellationToken>(), request.Id))
            .ReturnsAsync(true);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.NameAlreadyExists);

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTag_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateUpdateTagRequest(name: "  Updated  ", color: "  #00FF00  ");
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        TagRepositoryMock
            .Setup(x => x.ExistsByNameAsync(userId, request.Name, It.IsAny<CancellationToken>(), request.Id))
            .ReturnsAsync(false);

        TagRepositoryMock
            .Setup(x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.UpdateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tag.Id);
        tag.Name.Should().Be("Updated");
        tag.Color.Should().Be("#00FF00");

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTagWasNotFound()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tagId = Guid.NewGuid();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await sut.DeleteAsync(tagId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagNotFound);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTagBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = TagTestData.UserId;
        var tagId = Guid.NewGuid();
        var tag = TagTestData.CreateTag(TagTestData.AnotherUserId, "Important", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.DeleteAsync(tagId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.AccessDenied);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenTagAlreadyDeleted()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tagId = Guid.NewGuid();
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");
        tag.Delete();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.DeleteAsync(tagId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagAlreadyDeleted);

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTag_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tagId = Guid.NewGuid();
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        TagRepositoryMock
            .Setup(x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.DeleteAsync(tagId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tag.Id);
        tag.IsDeleted.Should().BeTrue();

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldReturnFailure_WhenTagWasNotFound()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateAssignCategoriesRequest();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(request.TagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await sut.AssignCategoriesAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagNotFound);
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldReturnFailure_WhenTagBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = TagTestData.UserId;
        var request = TagTestData.CreateAssignCategoriesRequest();
        var tag = TagTestData.CreateTag(TagTestData.AnotherUserId, "Important", "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(request.TagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.AssignCategoriesAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.AccessDenied);
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldReturnFailure_WhenTagIsDeleted()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var request = TagTestData.CreateAssignCategoriesRequest();
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");
        tag.Delete();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(request.TagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await sut.AssignCategoriesAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagDomainErrors.TagDeleted);
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldReturnFailure_WhenNotAllCategoriesWereFound()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var category1 = Guid.NewGuid();
        var category2 = Guid.NewGuid();
        var request = TagTestData.CreateAssignCategoriesRequest(categoryIds: new[] { category1, category2 });
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");

        var foundCategories = new[]
        {
            CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#111111")
        };

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(request.TagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(foundCategories);

        var result = await sut.AssignCategoriesAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NotFound);

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldFilterEmptyAndDuplicateCategoryIds_WhenAssigning()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var request = TagTestData.CreateAssignCategoriesRequest(
            tagId: Guid.NewGuid(),
            categoryIds: new[] { Guid.Empty, categoryId1, categoryId1, categoryId2, Guid.Empty });

        IReadOnlyCollection<Guid>? capturedCategoryIds = null;

        var categories = new[]
        {
            CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#111111"),
            CategoryTestData.CreateCategory(userId, "Auto", CategoryType.Credit, "#222222")
        };

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(request.TagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, IReadOnlyCollection<Guid>, CancellationToken>((_, ids, _) => capturedCategoryIds = ids)
            .ReturnsAsync(categories);

        TagRepositoryMock
            .Setup(x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.AssignCategoriesAsync(request);

        result.IsSuccess.Should().BeTrue();
        capturedCategoryIds.Should().NotBeNull();
        capturedCategoryIds.Should().HaveCount(2);
        capturedCategoryIds.Should().OnlyHaveUniqueItems();
        capturedCategoryIds.Should().NotContain(Guid.Empty);

        tag.TagCategories.Should().HaveCount(2);

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AssignCategoriesAsync_ShouldAssignCategories_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = TagTestData.UserId;
        var tag = TagTestData.CreateTag(userId, "Important", "#FF0000");
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var request = TagTestData.CreateAssignCategoriesRequest(
            tagId: Guid.NewGuid(),
            categoryIds: new[] { categoryId1, categoryId2 });

        var categories = new[]
        {
            CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#111111"),
            CategoryTestData.CreateCategory(userId, "Auto", CategoryType.Credit, "#222222")
        };

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        TagRepositoryMock
            .Setup(x => x.GetByIdWithCategoriesAsync(request.TagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        TagRepositoryMock
            .Setup(x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.AssignCategoriesAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tag.Id);
        tag.TagCategories.Should().HaveCount(2);

        TagRepositoryMock.Verify(
            x => x.UpdateAsync(tag, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}