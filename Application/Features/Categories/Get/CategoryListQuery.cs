using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Get;

public sealed class CategoryListQuery
{
    public Guid UserId { get; init; }

    public string? Search { get; init; }

    public string SortBy { get; init; } = CategorySortFields.Name;

    public string SortDirection { get; init; } = ListQueryDefaults.SortAscending;

    public CategoryType? Type { get; init; }

    public bool IncludeDeleted { get; init; }

    public int Page { get; init; } = ListQueryDefaults.DefaultPage;

    public int Size { get; init; } = ListQueryDefaults.DefaultPageSize;
}