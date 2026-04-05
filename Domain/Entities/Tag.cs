namespace MSaver.Domain.Entities;

public sealed class Tag : Entity
{
    private Tag()
    {
        Name = null!;
        User = null!;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public string? Color { get; private set; }

    public bool IsDeleted { get; private set; }

    public User User { get; private set; }

    public ICollection<TransactionTag> TransactionTags { get; private set; } = new List<TransactionTag>();

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

    public void Restore()
    {
        if (!IsDeleted)
            throw new DomainException(TagDomainErrors.TagAlreadyActive);

        IsDeleted = false;
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