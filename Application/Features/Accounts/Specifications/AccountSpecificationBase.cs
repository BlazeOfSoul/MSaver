using MSaver.Application.Common.Specifications;
using MSaver.Application.Features.Accounts.Get;

namespace MSaver.Application.Features.Accounts.Specifications;

public abstract class AccountSpecificationBase : BaseSpecification<Account>
{
    protected void ApplyBaseFilters(AccountListQuery query)
    {
        AddCriteriaInternal(x => x.UserId == query.UserId);

        if (query.IsArchived.HasValue)
            AddCriteriaInternal(x => x.IsArchived == query.IsArchived.Value);

        if (!string.IsNullOrWhiteSpace(query.CurrencyCode))
            AddCriteriaInternal(x => x.CurrencyCode == query.CurrencyCode);

        this.ApplyContainsIfNotEmpty(query.Search, search => x => x.Name.Contains(search));
    }

    protected void ApplySorting(string? sortBy, string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(sortBy, AccountSortFields.CreatedAt);
        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        switch (normalizedSortBy)
        {
            case var field when field.Equals(AccountSortFields.Name, StringComparison.OrdinalIgnoreCase):
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.Name);
                if (normalizedSortDirection == ListQueryDefaults.SortDescending)
                    this.AddTieBreakerByIdDescending(x => x.Id);
                else
                    this.AddTieBreakerByIdAscending(x => x.Id);
                break;

            case var field when field.Equals(AccountSortFields.CurrencyCode, StringComparison.OrdinalIgnoreCase):
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.CurrencyCode);
                if (normalizedSortDirection == ListQueryDefaults.SortDescending)
                    this.AddTieBreakerByIdDescending(x => x.Id);
                else
                    this.AddTieBreakerByIdAscending(x => x.Id);
                break;

            default:
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.CreatedAt);
                if (normalizedSortDirection == ListQueryDefaults.SortDescending)
                    this.AddTieBreakerByIdDescending(x => x.Id);
                else
                    this.AddTieBreakerByIdAscending(x => x.Id);
                break;
        }
    }
}