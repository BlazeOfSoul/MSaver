namespace MSaver.Application.Common.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
        where T : class
    {
        var query = inputQuery;

        if (specification.Criteria is not null)
            query = query.Where(specification.Criteria);

        if (specification.AsNoTracking)
            query = query.AsNoTracking();

        query = specification.Includes.Aggregate(
            query,
            static (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
        {
            IOrderedQueryable<T> orderedQuery = query.OrderBy(specification.OrderBy);

            foreach (var thenBy in specification.ThenByExpressions)
                orderedQuery = orderedQuery.ThenBy(thenBy);

            foreach (var thenByDescending in specification.ThenByDescendingExpressions)
                orderedQuery = orderedQuery.ThenByDescending(thenByDescending);

            query = orderedQuery;
        }
        else if (specification.OrderByDescending is not null)
        {
            IOrderedQueryable<T> orderedQuery = query.OrderByDescending(specification.OrderByDescending);

            foreach (var thenBy in specification.ThenByExpressions)
                orderedQuery = orderedQuery.ThenBy(thenBy);

            foreach (var thenByDescending in specification.ThenByDescendingExpressions)
                orderedQuery = orderedQuery.ThenByDescending(thenByDescending);

            query = orderedQuery;
        }

        if (specification.IsPagingEnabled && specification.Skip.HasValue && specification.Take.HasValue)
            query = query.Skip(specification.Skip.Value).Take(specification.Take.Value);

        return query;
    }
}