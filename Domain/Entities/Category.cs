using System.ComponentModel.DataAnnotations.Schema;

using MSaver.Domain.Enums;

namespace MSaver.Domain.Entities;

public sealed class Category : Entity
{
    private readonly List<Transaction> _transactions = [];
    private readonly List<TagCategory> _tagCategories = [];

    private Category() { }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string Name { get; private set; } = null!;

    public CategoryType Type { get; private set; }

    public string Color { get; private set; } = null!;

    public bool IsDeleted { get; private set; }

    [NotMapped]
    public bool IsSystem =>
        Type == CategoryType.TransferIncome ||
        Type == CategoryType.TransferExpense;

    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    public IReadOnlyCollection<TagCategory> TagCategories => _tagCategories;

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

    public void Update(
        string name,
        string color,
        CategoryType type)
    {
        if (IsSystem)
            throw new DomainException(CategoryDomainErrors.SystemCategoryCannotBeModified);

        SetName(name);
        SetColor(color);

        Type = type;
    }

    public void SoftDelete()
    {
        if (IsSystem)
            throw new DomainException(CategoryDomainErrors.SystemCategoryCannotBeDeleted);

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
}