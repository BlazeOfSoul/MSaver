using MSaver.Application.Features.Transactions.Get;

namespace MSaver.Application.Features.Transactions.Specifications;

public sealed class TransactionsCountSpecification : TransactionSpecificationBase
{
    public TransactionsCountSpecification(TransactionListQuery query)
    {
        ApplyBaseFilters(query);
    }
}