using MSaver.Api.Contracts.Categories;
using MSaver.Application.Features.Categories.Get;
using MSaver.Domain.Common;
using MSaver.Domain.Enums;
using MSaver.UnitTests.Common;
using MSaver.UnitTests.Common.TestData;

namespace MSaver.UnitTests.Services;

public sealed class CategoryServiceTests : CategoryServiceTestBase
{
    [Fact]
    public async Task GetAsync_ShouldReturnMappedPagedCategories_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;

        var request = CategoryTestData.CreateGetCategoriesRequest(
            search: " food ",
            sortBy: CategorySortFields.Name,
            sortDirection: ListQueryDefaults.SortAscending,
            type: "Debit",
            page: 2,
            size: 5);

        var categories = new[]
        {
            CategoryTestData.CreateCategory(userId, "Food", CategoryType.Debit, "#FF0000"),
            CategoryTestData.CreateCategory(userId, "Salary", CategoryType.Credit, "#00FF00")
        };

        var pagedCategories = CategoryTestData.CreatePagedCategories(
            categories,
            page: 2,
            size: 5,
            totalCount: 12);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<CategoryListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedCategories);

        var result = await sut.GetAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Items.Should().HaveCount(2);
        response.Page.Should().Be(2);
        response.Size.Should().Be(5);
        response.TotalCount.Should().Be(12);
        response.TotalPages.Should().Be(pagedCategories.TotalPages);
        response.HasPreviousPage.Should().BeTrue();
        response.HasNextPage.Should().BeTrue();

        response.Items.First().Name.Should().Be("Food");
        response.Items.First().Type.Should().Be(CategoryType.Debit);
        response.Items.First().Color.Should().Be("#FF0000");

        response.Items.Last().Name.Should().Be("Salary");
        response.Items.Last().Type.Should().Be(CategoryType.Credit);
        response.Items.Last().Color.Should().Be("#00FF00");
    }

    [Fact]
    public async Task GetAsync_ShouldPassParsedTypeToRepository_WhenTypeCanBeParsed()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateGetCategoriesRequest(type: "Credit");

        CategoryListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<CategoryListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(CategoryTestData.CreatePagedCategories([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Type.Should().Be(CategoryType.Credit);
    }

    [Fact]
    public async Task GetAsync_ShouldPassNullTypeToRepository_WhenTypeIsInvalid()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateGetCategoriesRequest(type: "SomeInvalidType");

        CategoryListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<CategoryListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(CategoryTestData.CreatePagedCategories([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.Type.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAsync_ShouldPassNullType_WhenTypeIsEmpty(string? type)
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateGetCategoriesRequest(type: type);

        CategoryListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<CategoryListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(CategoryTestData.CreatePagedCategories([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.Type.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldNormalizeQueryFields_WhenBuildingQuery()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;

        var request = new GetCategoriesRequest
        {
            Search = "  food  ",
            SortBy = "  type  ",
            SortDirection = "ASC",
            Type = "debit",
            Page = 3,
            Size = 15
        };

        CategoryListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<CategoryListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(CategoryTestData.CreatePagedCategories([], page: 3, size: 15, totalCount: 0));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Search.Should().Be("food");
        capturedQuery.SortBy.Should().Be("type");
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortAscending);
        capturedQuery.Type.Should().Be(CategoryType.Debit);
        capturedQuery.Page.Should().Be(3);
        capturedQuery.Size.Should().Be(15);
    }

    [Fact]
    public async Task GetAsync_ShouldUseDefaultSortAndPaging_WhenRequestHasDefaultValues()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;

        var request = new GetCategoriesRequest();

        CategoryListQuery? capturedQuery = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<CategoryListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryListQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(CategoryTestData.CreatePagedCategories([]));

        await sut.GetAsync(request);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be(userId);
        capturedQuery.Search.Should().BeNull();
        capturedQuery.SortBy.Should().Be(CategorySortFields.Name);
        capturedQuery.SortDirection.Should().Be(ListQueryDefaults.SortAscending);
        capturedQuery.Type.Should().BeNull();
        capturedQuery.Page.Should().Be(ListQueryDefaults.DefaultPage);
        capturedQuery.Size.Should().Be(ListQueryDefaults.DefaultPageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenCategoryWasNotFound()
    {
        var sut = CreateSut();
        var categoryId = Guid.NewGuid();
        var userId = CategoryTestData.UserId;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await sut.GetByIdAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenCategoryBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var categoryId = Guid.NewGuid();
        var currentUserId = CategoryTestData.UserId;

        var category = CategoryTestData.CreateCategory(
            userId: CategoryTestData.AnotherUserId,
            name: "Food",
            type: CategoryType.Debit);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.GetByIdAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.AccessDenied);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedCategory_WhenCategoryExistsAndBelongsToCurrentUser()
    {
        var sut = CreateSut();
        var categoryId = Guid.NewGuid();
        var userId = CategoryTestData.UserId;

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Food",
            type: CategoryType.Debit,
            color: "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.GetByIdAsync(categoryId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var response = result.Value!;
        response.Name.Should().Be(category.Name);
        response.Type.Should().Be(category.Type);
        response.Color.Should().Be(category.Color);
        response.IsDeleted.Should().BeFalse();
        response.IsSystem.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnIsSystemTrue_WhenCategoryIsSystem()
    {
        var sut = CreateSut();
        var categoryId = Guid.NewGuid();
        var userId = CategoryTestData.UserId;

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Transfer In",
            type: CategoryType.TransferIncome,
            color: "#123456");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.GetByIdAsync(categoryId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsSystem.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenCategoryNameAlreadyExists()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateCategoryRequest(name: "Food");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(true);

        var result = await sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NameAlreadyExists);

        CategoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        CategoryRepositoryMock.Verify(
            x => x.ExistsByNameAsync(userId, request.Name, It.IsAny<CancellationToken>(), null),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateCategoryRequest(
            name: "  Food  ",
            type: CategoryType.Debit,
            color: "  #FF0000  ");

        Category? createdCategory = null;

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(false);

        CategoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((category, _) => createdCategory = category)
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        createdCategory.Should().NotBeNull();
        createdCategory!.UserId.Should().Be(userId);
        createdCategory.Name.Should().Be("Food");
        createdCategory.Type.Should().Be(CategoryType.Debit);
        createdCategory.Color.Should().Be("#FF0000");
        createdCategory.IsDeleted.Should().BeFalse();

        CategoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenCategoryWasNotFound()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateUpdateCategoryRequest();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NotFound);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenCategoryIsDeleted()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateUpdateCategoryRequest();

        var category = CategoryTestData.CreateCategory(userId: userId);
        category.SoftDelete();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.CategoryDeleted);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenCategoryBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = CategoryTestData.UserId;
        var request = CategoryTestData.CreateUpdateCategoryRequest();

        var category = CategoryTestData.CreateCategory(userId: CategoryTestData.AnotherUserId);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.AccessDenied);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenAnotherCategoryWithSameNameExists()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();
        var request = CategoryTestData.CreateUpdateCategoryRequest(
            id: categoryId,
            name: "Food",
            color: "#00FF00",
            type: CategoryType.Credit);

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Old name",
            type: CategoryType.Debit,
            color: "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        CategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                request.Id))
            .ReturnsAsync(true);

        var result = await sut.UpdateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NameAlreadyExists);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenRequestIsValid()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        var request = CategoryTestData.CreateUpdateCategoryRequest(
            id: categoryId,
            name: "  Updated Food  ",
            color: "  #00FF00  ",
            type: CategoryType.Credit);

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Food",
            type: CategoryType.Debit,
            color: "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        CategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                request.Id))
            .ReturnsAsync(false);

        CategoryRepositoryMock
            .Setup(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.UpdateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(category.Id);

        category.Name.Should().Be("Updated Food");
        category.Color.Should().Be("#00FF00");
        category.Type.Should().Be(CategoryType.Credit);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowDomainException_WhenCategoryIsSystem()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        var request = CategoryTestData.CreateUpdateCategoryRequest(
            id: categoryId,
            name: "Updated",
            color: "#00FF00",
            type: CategoryType.Credit);

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Transfer In",
            type: CategoryType.TransferIncome,
            color: "#123456");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        CategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                userId,
                request.Name,
                It.IsAny<CancellationToken>(),
                request.Id))
            .ReturnsAsync(false);

        Func<Task> act = async () => await sut.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>();

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenCategoryWasNotFound()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await sut.DeleteAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenCategoryIsDeleted()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        var category = CategoryTestData.CreateCategory(userId: userId);
        category.SoftDelete();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.DeleteAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.CategoryDeleted);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenCategoryBelongsToAnotherUser()
    {
        var sut = CreateSut();
        var currentUserId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        var category = CategoryTestData.CreateCategory(userId: CategoryTestData.AnotherUserId);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await sut.DeleteAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryDomainErrors.AccessDenied);

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteCategory_WhenCategoryBelongsToCurrentUser()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Food",
            type: CategoryType.Debit,
            color: "#FF0000");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        CategoryRepositoryMock
            .Setup(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await sut.DeleteAsync(categoryId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(category.Id);
        category.IsDeleted.Should().BeTrue();

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowDomainException_WhenCategoryIsSystem()
    {
        var sut = CreateSut();
        var userId = CategoryTestData.UserId;
        var categoryId = Guid.NewGuid();

        var category = CategoryTestData.CreateCategory(
            userId: userId,
            name: "Transfer Out",
            type: CategoryType.TransferExpense,
            color: "#654321");

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        Func<Task> act = async () => await sut.DeleteAsync(categoryId);

        await act.Should().ThrowAsync<DomainException>();

        CategoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}