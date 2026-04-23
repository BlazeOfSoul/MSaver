using MSaver.Application.Common.Models;

namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionListQuery
{
    public Guid UserId { get; init; }

    public Guid? AccountId { get; init; }

    public Guid? CategoryId { get; init; }

    public DateTime? FromDate { get; init; }

    public DateTime? ToDate { get; init; }

    public string? Search { get; init; }

    public string SortBy { get; init; } = TransactionSortFields.Date;

    public string SortDirection { get; init; } = ListQueryDefaults.SortDescending;

    public int Page { get; init; } = ListQueryDefaults.DefaultPage;

    public int Size { get; init; } = ListQueryDefaults.DefaultPageSize;
}