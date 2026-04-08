namespace MSaver.Domain.Entities;

public sealed class Transaction : Entity
{
    private readonly List<TransactionTag> _transactionTags = [];

    private Transaction() { }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public Guid AccountId { get; private set; }
    public Account? Account { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public Guid CurrencyId { get; private set; }
    public Currency? Currency { get; private set; }

    public Guid? BaseCurrencyId { get; private set; }
    public Currency? BaseCurrency { get; private set; }

    public Guid? TransferId { get; private set; }

    public decimal Amount { get; private set; }

    public decimal? AmountBase { get; private set; }

    public decimal? TransferRate { get; private set; }

    public DateTime Date { get; private set; }

    public string Description { get; private set; } = null!;

    public IReadOnlyCollection<TransactionTag> TransactionTags => _transactionTags;

    public static Transaction Create(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        Guid currencyId,
        decimal amount,
        DateTime date,
        string? description = null,
        Guid? baseCurrencyId = null,
        decimal? amountBase = null)
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
            BaseCurrencyId = baseCurrencyId,
            Date = date,
            AmountBase = amountBase
        };

        transaction.SetAmount(amount);
        transaction.SetDescription(description);

        return transaction;
    }

    public void Update(
        Guid categoryId,
        decimal amount,
        DateTime date,
        string? description = null,
        Guid? baseCurrencyId = null,
        decimal? amountBase = null)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.CategoryIdRequired);

        CategoryId = categoryId;
        Date = date;
        BaseCurrencyId = baseCurrencyId;
        AmountBase = amountBase;

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

    public void SetBaseAmount(Guid baseCurrencyId, decimal amountBase)
    {
        if (baseCurrencyId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.BaseCurrencyIdRequired);

        BaseCurrencyId = baseCurrencyId;
        AmountBase = amountBase;
    }

    public void MarkAsTransfer(Guid transferId, decimal transferRate)
    {
        if (transferId == Guid.Empty)
            throw new DomainException(TransactionDomainErrors.TransferIdRequired);

        if (transferRate <= 0)
            throw new DomainException(TransactionDomainErrors.TransferRateMustBePositive);

        TransferId = transferId;
        TransferRate = transferRate;
    }

    public void ClearTransfer()
    {
        TransferId = null;
        TransferRate = null;
    }

    public void ReplaceTags(IEnumerable<Guid> tagIds)
    {
        _transactionTags.Clear();

        foreach (var tagId in tagIds.Distinct())
        {
            _transactionTags.Add(TransactionTag.Create(Id, tagId));
        }
    }

    public bool IsTransfer()
    {
        return TransferId.HasValue;
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