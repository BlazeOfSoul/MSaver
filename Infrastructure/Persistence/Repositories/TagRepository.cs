using MSaver.Application.Features.Tags.Get;
using MSaver.Application.Features.Tags.Specifications;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class TagRepository(ApplicationDbContext context) : EfRepositoryBase<Tag>(context), ITagRepository
{
    public Task<Tag?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync(new TagByIdSpecification(id), cancellationToken);
    }

    public Task<Tag?> GetByIdWithCategoriesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Context.Tags
            .AsNoTracking()
            .Include(x => x.TagCategories)
            .ThenInclude(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Tag>> GetPagedAsync(
        TagListQuery query,
        CancellationToken cancellationToken = default)
    {
        var listSpecification = new TagsListSpecification(query);
        var countSpecification = new TagsCountSpecification(query);

        var totalCount = await CountAsync(countSpecification, cancellationToken);
        var items = await ListAsync(listSpecification, cancellationToken);

        return new PagedResult<Tag>
        {
            Items = items,
            Page = query.Page,
            Size = query.Size,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        await Context.Tags.AddAsync(tag, cancellationToken);
    }

    public Task UpdateAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        Context.Tags.Update(tag);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = Context.Tags
            .Where(x => x.UserId == userId && !x.IsDeleted && x.Name == normalizedName);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
