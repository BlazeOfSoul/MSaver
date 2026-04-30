using System.Linq.Expressions;

namespace MSaver.Application.Common.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    public List<Expression<Func<T, object>>> Includes { get; } = [];

    public Expression<Func<T, object>>? OrderBy { get; private set; }

    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    public List<Expression<Func<T, object>>> ThenByExpressions { get; } = [];

    public List<Expression<Func<T, object>>> ThenByDescendingExpressions { get; } = [];

    public int? Skip { get; private set; }

    public int? Take { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    public bool AsNoTracking { get; private set; } = true;

    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        if (Criteria is null)
        {
            Criteria = criteria;
            return;
        }

        var parameter = Expression.Parameter(typeof(T));
        var leftVisitor = new ReplaceExpressionVisitor(Criteria.Parameters[0], parameter);
        var rightVisitor = new ReplaceExpressionVisitor(criteria.Parameters[0], parameter);

        var left = leftVisitor.Visit(Criteria.Body)!;
        var right = rightVisitor.Visit(criteria.Body)!;
        var body = Expression.AndAlso(left, right);

        Criteria = Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> expression)
    {
        OrderBy = expression;
        OrderByDescending = null;
        ThenByExpressions.Clear();
        ThenByDescendingExpressions.Clear();
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> expression)
    {
        OrderByDescending = expression;
        OrderBy = null;
        ThenByExpressions.Clear();
        ThenByDescendingExpressions.Clear();
    }

    protected void AddThenBy(Expression<Func<T, object>> expression)
    {
        ThenByExpressions.Add(expression);
    }

    protected void AddThenByDescending(Expression<Func<T, object>> expression)
    {
        ThenByDescendingExpressions.Add(expression);
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyPage(int page, int size)
    {
        ApplyPaging((page - 1) * size, size);
    }

    protected void ApplyTracking()
    {
        AsNoTracking = false;
    }

    internal void AddCriteriaInternal(Expression<Func<T, bool>> criteria) => AddCriteria(criteria);

    internal void AddIncludeInternal(Expression<Func<T, object>> includeExpression) => AddInclude(includeExpression);

    internal void ApplyOrderByInternal(Expression<Func<T, object>> expression) => ApplyOrderBy(expression);

    internal void ApplyOrderByDescendingInternal(Expression<Func<T, object>> expression) => ApplyOrderByDescending(expression);

    internal void AddThenByInternal(Expression<Func<T, object>> expression) => AddThenBy(expression);

    internal void AddThenByDescendingInternal(Expression<Func<T, object>> expression) => AddThenByDescending(expression);

    internal void ApplyPageInternal(int page, int size) => ApplyPage(page, size);
}