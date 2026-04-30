namespace MSaver.Application.Features.Accounts.Get;

public sealed class AccountListQuery
{
    public Guid UserId { get; init; }

    public string? Search { get; init; }

    public string SortBy { get; init; } = "createdAt";

    public string SortDirection { get; init; } = "desc";

    public bool? IsArchived { get; init; }

    public string? CurrencyCode { get; init; }

    public int Page { get; init; } = 1;

    public int Size { get; init; } = 20;
}