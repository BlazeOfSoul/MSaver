using server.Domain.Common;
using server.Domain.Enums;
using server.Domain.Errors;

namespace server.Domain.Entities;

public sealed class Category : Entity
{
    private Category()
    {
        Name = string.Empty;
        Color = "#ffffff";
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public CategoryType Type { get; private set; }
    public string Color { get; private set; } = "#ffffff";
    public bool IsDeleted { get; private set; }

    public User User { get; private set; } = null!;

    public Category(Guid userId, string name, CategoryType type, string color)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(DomainErrors.Category.UserIdRequired, nameof(userId));

        UserId = userId;
        Type = type;
        IsDeleted = false;

        SetName(name);
        SetColor(color);
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
            throw new ArgumentException(DomainErrors.Category.NameRequired, nameof(name));

        Name = name;
    }

    private void SetColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException(DomainErrors.Category.ColorRequired, nameof(color));

        Color = color;
    }
}
