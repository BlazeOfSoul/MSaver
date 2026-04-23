using MSaver.Api.Contracts.Common;
using MSaver.Application.Features.Tags.Get;

namespace MSaver.Api.Contracts.Tags;

public sealed class GetTagsRequest : PagedRequest
{
    public string? Search { get; init; }

    public string? SortBy { get; init; } = TagSortFields.Name;

    public string? SortDirection { get; init; } = ListQueryDefaults.SortAscending;

    public bool? IncludeDeleted { get; init; } = false;
}