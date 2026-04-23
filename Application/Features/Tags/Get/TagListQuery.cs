namespace MSaver.Application.Features.Tags.Get;

public sealed class TagListQuery
{
    public Guid UserId { get; init; }

    public string? Search { get; init; }

    public bool IncludeDeleted { get; init; }

    public string SortBy { get; init; } = TagSortFields.Name;

    public string SortDirection { get; init; } = ListQueryDefaults.SortAscending;

    public int Page { get; init; } = ListQueryDefaults.DefaultPage;

    public int Size { get; init; } = ListQueryDefaults.DefaultPageSize;
}