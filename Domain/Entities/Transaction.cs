namespace MSaver.Domain.Entities;

public sealed class Transaction : Entity
{
    private Transaction() { }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public Guid AccountId { get; private set; }
    public Account? Account { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public Guid CurrencyId { get; private set; }
    public Currency? Currency { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime Date { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public static Transaction Create(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        Guid currencyId,
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

        if (currencyId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.CurrencyIdRequired);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            CurrencyId = currencyId,
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

    public void ChangeAccount(Guid accountId, Guid currencyId)
    {
        if (accountId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.AccountIdRequired);

        if (currencyId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.CurrencyIdRequired);

        AccountId = accountId;
        CurrencyId = currencyId;
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