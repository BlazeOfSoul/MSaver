using MSaver.Application.Common.Specifications;
using MSaver.Infrastructure.Persistence.Specifications;

namespace MSaver.Infrastructure.Persistence.Repositories;

public abstract class EfRepositoryBase<T>(ApplicationDbContext context) where T : Entity
{
    protected readonly ApplicationDbContext Context = context;

    protected IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        return SpecificationEvaluator.GetQuery(Context.Set<T>().AsQueryable(), specification);
    }

    protected Task<T?> FirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    protected Task<List<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    protected async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>().AsQueryable();

        if (specification.Criteria is not null)
            query = query.Where(specification.Criteria);

        return await query.CountAsync(cancellationToken);
    }
}