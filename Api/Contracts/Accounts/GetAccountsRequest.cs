using MSaver.Api.Contracts.Common;
using MSaver.Application.Features.Accounts.Get;

namespace MSaver.Api.Contracts.Accounts;

public sealed class GetAccountsRequest : PagedRequest
{
    public string? Search { get; init; }

    public string? SortBy { get; init; } = AccountSortFields.CreatedAt;

    public string? SortDirection { get; init; } = ListQueryDefaults.SortDescending;

    public bool? IsArchived { get; init; }

    public string? CurrencyCode { get; init; }
}