using MSaver.Application.Features.Accounts.Get;

namespace MSaver.Application.Features.Accounts.Specifications;

public sealed class AccountsCountSpecification : AccountSpecificationBase
{
    public AccountsCountSpecification(AccountListQuery query)
    {
        ApplyBaseFilters(query);
    }
}