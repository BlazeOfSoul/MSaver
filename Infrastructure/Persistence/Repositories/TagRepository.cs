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
            .Include(x => x.TagCategories)
            .ThenInclude(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tag>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
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
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

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
}