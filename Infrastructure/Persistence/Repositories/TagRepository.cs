using MSaver.Application.Features.Tags.Get;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class TagRepository(ApplicationDbContext dbContext) : ITagRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Tag?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Tag?> GetByIdWithCategoriesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .Include(x => x.TagCategories)
            .ThenInclude(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Tag>> GetPagedAsync(
        TagListQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tag> dbQuery = _dbContext.Tags
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId);

        if (!query.IncludeDeleted)
            dbQuery = dbQuery.Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            dbQuery = dbQuery.Where(x => x.Name.Contains(search));
        }

        dbQuery = ApplySorting(
            dbQuery,
            query.SortBy,
            query.SortDirection);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        return new PagedResult<Tag>
        {
            Items = items,
            Page = query.Page,
            Size = query.Size,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var query = _dbContext.Tags
            .Where(x => x.UserId == userId && x.Name == name && !x.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Tags.AddAsync(tag, cancellationToken);
    }

    public Task UpdateAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Tags.Update(tag);
        return Task.CompletedTask;
    }

    private static IQueryable<Tag> ApplySorting(
        IQueryable<Tag> query,
        string? sortBy,
        string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(
            sortBy,
            TagSortFields.Name);

        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        return (normalizedSortBy, normalizedSortDirection) switch
        {
            (var field, var direction)
                when field.Equals(TagSortFields.Name, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortDescending
                    => query
                        .OrderByDescending(x => x.Name)
                        .ThenBy(x => x.Id),

            _ => query
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
        };
    }
}