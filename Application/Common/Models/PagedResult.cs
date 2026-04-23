namespace MSaver.Application.Common.Models;

public sealed class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = [];

    public int Page { get; init; }

    public int Size { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)Size);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}