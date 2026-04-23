using MSaver.Api.Contracts.Common;
using MSaver.Application.Features.Categories.Get;

namespace MSaver.Api.Contracts.Categories;

public sealed class GetCategoriesRequest : PagedRequest
{
    public string? Search { get; init; }

    public string? SortBy { get; init; } = CategorySortFields.Name;

    public string? SortDirection { get; init; } = ListQueryDefaults.SortAscending;

    public string? Type { get; init; }

    public bool IncludeDeleted { get; init; } = false;
}