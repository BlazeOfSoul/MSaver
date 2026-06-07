using MSaver.Api.Contracts.Categories;
using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Update;
using MSaver.Domain.Enums;

namespace MSaver.UnitTests.Common.TestData;

public static class CategoryTestData
{
    public static Guid UserId => Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static Guid AnotherUserId => Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static Category CreateCategory(
        Guid? userId = null,
        string name = "Food",
        CategoryType type = CategoryType.Debit,
        string color = "#FF0000",
        DefaultCategoryType? defaultCategoryType = null)
    {
        return Category.Create(
            userId ?? UserId,
            name,
            type,
            color,
            defaultCategoryType);
    }

    public static CreateCategoryRequest CreateCategoryRequest(
        string name = "Food",
        CategoryType type = CategoryType.Debit,
        string color = "#FF0000")
    {
        return new CreateCategoryRequest
        {
            Name = name,
            Type = type,
            Color = color
        };
    }

    public static UpdateCategoryRequest CreateUpdateCategoryRequest(
        Guid? id = null,
        string name = "Updated Food",
        string color = "#00FF00",
        CategoryType type = CategoryType.Credit)
    {
        return new UpdateCategoryRequest(
            id ?? Guid.NewGuid(),
            name,
            color,
            type);
    }

    public static GetCategoriesRequest CreateGetCategoriesRequest(
        string? search = " food ",
        string? sortBy = CategorySortFields.Name,
        string? sortDirection = ListQueryDefaults.SortAscending,
        string? type = null,
        int page = 1,
        int size = 20)
    {
        return new GetCategoriesRequest
        {
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Type = type,
            Page = page,
            Size = size
        };
    }

    public static PagedResult<Category> CreatePagedCategories(
        IReadOnlyCollection<Category> items,
        int page = 1,
        int size = 20,
        int? totalCount = null)
    {
        return new PagedResult<Category>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = totalCount ?? items.Count
        };
    }
}
