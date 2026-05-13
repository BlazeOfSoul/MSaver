using MSaver.Application.Common.Specifications;

namespace MSaver.Application.Features.Accounts.Specifications;

public sealed class AccountByIdSpecification : BaseSpecification<Account>
{
    public AccountByIdSpecification(Guid id)
    {
        AddCriteria(x => x.Id == id);
    }
}