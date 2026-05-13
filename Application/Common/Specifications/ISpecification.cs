using System.Linq.Expressions;

namespace MSaver.Application.Common.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    List<Expression<Func<T, object>>> Includes { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    List<Expression<Func<T, object>>> ThenByExpressions { get; }

    List<Expression<Func<T, object>>> ThenByDescendingExpressions { get; }

    int? Skip { get; }

    int? Take { get; }

    bool IsPagingEnabled { get; }

    bool AsNoTracking { get; }
}