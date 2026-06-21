using MSaver.Domain.Constants;

namespace MSaver.Domain.Entities;

public sealed class Account : AuditableEntity
{
    private readonly List<Transaction> _transactions = [];

    private Account() { }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string CurrencyCode { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Color { get; private set; }

    public decimal InitialBalance { get; private set; }

    public bool IsPrimary { get; private set; }

    public bool IsArchived { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    public static Account Create(
        Guid userId,
        string currencyCode,
        string name,
        string? color = null,
        bool isPrimary = false,
        decimal initialBalance = 0m)
    {
        if (userId == Guid.Empty)
            throw new DomainException(AccountDomainErrors.UserIdRequired);

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new DomainException(AccountDomainErrors.CurrencyCodeRequired);

        if (initialBalance < 0)
            throw new DomainException(AccountDomainErrors.InitialBalanceNegative);

        var account = new Account
        {
            UserId = userId,
            InitialBalance = initialBalance,
            IsPrimary = isPrimary,
            IsArchived = false
        };

        account.SetCurrencyCode(currencyCode);
        account.SetName(name);
        account.SetColor(color);

        return account;
    }

    public void Update(
        string name,
        string? color = null)
    {
        SetName(name);
        SetColor(color);
    }

    public void Archive()
    {
        if (IsPrimary)
            throw new DomainException(AccountDomainErrors.PrimaryAccountCannotBeArchived);

        IsArchived = true;
    }

    public void Restore()
    {
        IsArchived = false;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(AccountDomainErrors.NameRequired);

        Name = name.Trim();
    }

    private void SetColor(string? color)
    {
        Color = string.IsNullOrWhiteSpace(color)
            ? null
            : color.Trim();
    }

    private void SetCurrencyCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new DomainException(AccountDomainErrors.CurrencyCodeRequired);

        var normalizedCode = currencyCode.Trim().ToUpperInvariant();

        if (!CurrencyDefinitions.Exists(normalizedCode))
            throw new DomainException(AccountDomainErrors.CurrencyNotFound);

        CurrencyCode = normalizedCode;
    }
}
