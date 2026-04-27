using MSaver.Application.Common.Specifications;
using MSaver.Application.Features.Transactions.Get;

namespace MSaver.Application.Features.Transactions.Specifications;

public abstract class TransactionSpecificationBase : BaseSpecification<Transaction>
{
    protected void ApplyBaseFilters(TransactionListQuery query)
    {
        AddCriteriaInternal(x => x.UserId == query.UserId);

        if (query.AccountId.HasValue)
            AddCriteriaInternal(x => x.AccountId == query.AccountId.Value);

        if (query.CategoryId.HasValue)
            AddCriteriaInternal(x => x.CategoryId == query.CategoryId.Value);

        if (query.FromDate.HasValue)
            AddCriteriaInternal(x => x.Date >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            AddCriteriaInternal(x => x.Date < query.ToDate.Value);

        this.ApplyContainsIfNotEmpty(query.Search, search => x => x.Description.Contains(search));
    }

    protected void ApplyIncludes()
    {
        this.AddIncludePath(x => x.Account!);
        this.AddIncludePath(x => x.Category!);
    }

    protected void ApplySorting(string? sortBy, string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(sortBy, TransactionSortFields.Date);
        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        switch (normalizedSortBy)
        {
            case var field when field.Equals(TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase):
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.Amount);
                if (normalizedSortDirection == ListQueryDefaults.SortDescending)
                {
                    AddThenByDescendingInternal(x => x.Date);
                    AddThenByDescendingInternal(x => x.Id);
                }
                else
                {
                    AddThenByInternal(x => x.Date);
                    AddThenByInternal(x => x.Id);
                }
                break;

            default:
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.Date);
                if (normalizedSortDirection == ListQueryDefaults.SortDescending)
                    this.AddTieBreakerByIdDescending(x => x.Id);
                else
                    this.AddTieBreakerByIdAscending(x => x.Id);
                break;
        }
    }
}