using MSaver.Application.Common.Specifications;

namespace MSaver.Application.Features.Transactions.Specifications;

public sealed class TransactionByIdSpecification : BaseSpecification<Transaction>
{
    public TransactionByIdSpecification(Guid id)
    {
        AddCriteria(x => x.Id == id);
        AddInclude(x => x.Account!);
        AddInclude(x => x.Category!);
    }
}