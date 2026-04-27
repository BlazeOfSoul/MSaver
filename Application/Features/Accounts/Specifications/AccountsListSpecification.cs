using MSaver.Application.Features.Accounts.Get;

namespace MSaver.Application.Features.Accounts.Specifications;

public sealed class AccountsListSpecification : AccountSpecificationBase
{
    public AccountsListSpecification(AccountListQuery query)
    {
        ApplyBaseFilters(query);
        ApplySorting(query.SortBy, query.SortDirection);
        ApplyPage(query.Page, query.Size);
    }
}