using MSaver.Application.Features.Transactions.Get;

namespace MSaver.Application.Features.Transactions.Specifications;

public sealed class TransactionsListSpecification : TransactionSpecificationBase
{
    public TransactionsListSpecification(TransactionListQuery query)
    {
        ApplyBaseFilters(query);
        ApplyIncludes();
        ApplySorting(query.SortBy, query.SortDirection);
        ApplyPage(query.Page, query.Size);
    }
}