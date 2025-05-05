using server.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

