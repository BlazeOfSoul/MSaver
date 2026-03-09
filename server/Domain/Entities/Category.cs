using server.Domain.Enums;

namespace server.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string Color { get; set; } = "#ffffff";
    public bool IsDeleted { get; set; }

    public User User { get; set; } = null!;
}