namespace MSaver.Domain.Entities;

public sealed class TagCategory
{
    private TagCategory() { }

    public Guid TagId { get; private set; }
    public Tag? Tag { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public static TagCategory Create(Guid tagId, Guid categoryId)
    {
        if (tagId == Guid.Empty)
            throw new DomainException(TagDomainErrors.TagNotFound);

        if (categoryId == Guid.Empty)
            throw new DomainException(CategoryDomainErrors.NotFound);

        return new TagCategory
        {
            TagId = tagId,
            CategoryId = categoryId
        };
    }
}