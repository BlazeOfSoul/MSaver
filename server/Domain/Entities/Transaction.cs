using server.Domain.Common;
using server.Domain.Errors;

namespace server.Domain.Entities;

public sealed class Transaction : Entity
{
    private Transaction()
    {
        Description = null!;
    }

    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }

    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }

    public User User { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    public static Transaction Create(
        Guid userId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string description = "")
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(TransactionErrors.UserIdRequired, nameof(userId));

        if (categoryId == Guid.Empty)
            throw new ArgumentException(TransactionErrors.CategoryIdRequired, nameof(categoryId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), TransactionErrors.AmountMustBePositive);

        return new()
        {
            UserId = userId,
            CategoryId = categoryId,
            Amount = amount,
            Date = date,
            Description = description ?? string.Empty
        };
    }

    public void ChangeDescription(string description)
    {
        Description = description ?? string.Empty;
    }
}
