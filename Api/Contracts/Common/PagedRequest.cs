namespace MSaver.Api.Contracts.Common;

public abstract class PagedRequest
{
    public int Page { get; init; } = ListQueryDefaults.DefaultPage;

    public int Size { get; init; } = ListQueryDefaults.DefaultPageSize;
}