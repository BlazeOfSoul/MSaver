using MSaver.Domain.Enums;

namespace MSaver.Domain.Entities;

public sealed class Category : Entity
{
    private readonly List<Category> _children = new();
    private readonly List<Transaction> _transactions = new();

    private Category()
    {
        Name = null!;
        Color = null!;
        User = null!;
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public Guid? ParentId { get; private set; }
    public Category? Parent { get; private set; }

    public string Name { get; private set; }

    public CategoryType Type { get; private set; }

    public string Color { get; private set; }

    public string? Icon { get; private set; }

    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<Category> Children => _children;

    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    public static Category Create(
        Guid userId,
        string name,
        CategoryType type,
        string color,
        string? icon = null,
        Guid? parentId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException(CategoryDomainErrors.UserIdRequired);

        var category = new Category
        {
            UserId = userId,
            Type = type,
            ParentId = parentId,
            IsDeleted = false
        };

        category.SetName(name);
        category.SetColor(color);
        category.SetIcon(icon);

        return category;
    }

    public void Update(
        string name,
        string color,
        CategoryType type,
        string? icon = null,
        Guid? parentId = null)
    {
        SetName(name);
        SetColor(color);
        SetIcon(icon);
        SetParent(parentId);

        Type = type;
    }

    public void SetParent(Guid? parentId)
    {
        ParentId = parentId;
    }

    public void ChangeIcon(string? icon)
    {
        SetIcon(icon);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CategoryDomainErrors.NameRequired);

        Name = name.Trim();
    }

    private void SetColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException(CategoryDomainErrors.ColorRequired);

        Color = color.Trim();
    }

    private void SetIcon(string? icon)
    {
        Icon = string.IsNullOrWhiteSpace(icon)
            ? null
            : icon.Trim();
    }
}