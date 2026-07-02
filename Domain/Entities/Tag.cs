namespace MSaver.Domain.Entities;

public sealed class Tag : AuditableEntity
{
    private readonly List<TagCategory> _tagCategories = [];

    private Tag() { }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Color { get; private set; }

    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<TagCategory> TagCategories => _tagCategories;

    public static Tag Create(Guid userId, string name, string? color = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException(TagDomainErrors.UserIdRequired);

        var tag = new Tag
        {
            UserId = userId,
            IsDeleted = false
        };

        tag.SetName(name);
        tag.SetColor(color);

        return tag;
    }

    public void Update(string name, string? color = null)
    {
        if (IsDeleted)
            throw new DomainException(TagDomainErrors.TagDeleted);

        SetName(name);
        SetColor(color);
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException(TagDomainErrors.TagAlreadyDeleted);

        IsDeleted = true;
    }

    public void ReplaceCategories(IEnumerable<Guid> categoryIds)
    {
        if (IsDeleted)
            throw new DomainException(TagDomainErrors.TagDeleted);

        var nextCategoryIds = categoryIds.Distinct().ToHashSet();

        foreach (var categoryId in nextCategoryIds)
            _ = TagCategory.Create(Id, categoryId);

        _tagCategories.RemoveAll(x => !nextCategoryIds.Contains(x.CategoryId));

        var existingCategoryIds = _tagCategories
            .Select(x => x.CategoryId)
            .ToHashSet();

        foreach (var categoryId in nextCategoryIds.Except(existingCategoryIds))
        {
            _tagCategories.Add(TagCategory.Create(Id, categoryId));
        }
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(TagDomainErrors.NameRequired);

        Name = name.Trim();
    }

    private void SetColor(string? color)
    {
        Color = string.IsNullOrWhiteSpace(color)
            ? null
            : color.Trim();
    }
}
