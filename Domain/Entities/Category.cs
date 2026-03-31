using MSaver.Domain.Common;
using MSaver.Domain.Enums;
using MSaver.Domain.Errors;

namespace MSaver.Domain.Entities;

public sealed class Category : Entity
{
    private Category()
    {
        Name = null!;
        Color = null!;
        User = null!;
    }

    public Guid UserId
    {
        get; private set;
    }

    public string Name
    {
        get; private set;
    }
    public CategoryType Type
    {
        get; private set;
    }
    public string Color
    {
        get; private set;
    }
    public bool IsDeleted
    {
        get; private set;
    }

    public User User
    {
        get; private set;
    }

    public static Category Create(
        Guid userId,
        string name,
        CategoryType type,
        string color)
    {
        if (userId == Guid.Empty)
            throw new DomainException(CategoryDomainErrors.UserIdRequired);

        var category = new Category
        {
            UserId = userId,
            Type = type,
            IsDeleted = false
        };

        category.SetName(name);
        category.SetColor(color);

        return category;
    }

    public void Update(string name, string color, CategoryType type)
    {
        SetName(name);
        SetColor(color);
        Type = type;
    }

    public void SoftDelete() => IsDeleted = true;

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CategoryDomainErrors.NameRequired);

        Name = name;
    }

    private void SetColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException(CategoryDomainErrors.ColorRequired);

        Color = color;
    }
}
