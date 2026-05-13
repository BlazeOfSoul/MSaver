namespace MSaver.Domain.Entities;

public sealed class Transaction : AuditableEntity
{
    private Transaction() { }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public Guid AccountId { get; private set; }
    public Account? Account { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime Date { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public static Transaction Create(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string? description = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.UserIdRequired);

        if (accountId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.AccountIdRequired);

        if (categoryId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.CategoryIdRequired);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Date = date
        };

        transaction.SetAmount(amount);
        transaction.SetDescription(description);

        return transaction;
    }

    public void Update(
        Guid categoryId,
        decimal amount,
        DateTime date,
        string? description = null)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.CategoryIdRequired);

        CategoryId = categoryId;
        Date = date;

        SetAmount(amount);
        SetDescription(description);
    }

    private void SetAmount(decimal amount)
    {
        if (amount == 0)
            throw new DomainException(TransactionDomainErrors.AmountMustNotBeZero);

        Amount = amount;
    }

    private void SetDescription(string? description)
    {
        Description = description?.Trim() ?? string.Empty;
    }
}