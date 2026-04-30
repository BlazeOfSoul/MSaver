using MSaver.Api.Contracts.Common;
using MSaver.Application.Features.Transactions.Get;

namespace MSaver.Api.Contracts.Transactions;

public sealed class GetTransactionsRequest : PagedRequest
{
    public Guid? AccountId { get; init; }

    public Guid? CategoryId { get; init; }

    public DateTime? FromDate { get; init; }

    public DateTime? ToDate { get; init; }

    public string? Search { get; init; }

    public string? SortBy { get; init; } = TransactionSortFields.Date;

    public string? SortDirection { get; init; } = ListQueryDefaults.SortDescending;
}