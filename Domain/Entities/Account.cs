namespace MSaver.Domain.Entities;

public sealed class Account : Entity
{
    private readonly List<Transaction> _transactions = new();

    private Account()
    {
        Name = null!;
        User = null!;
        Currency = null!;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public Guid CurrencyId { get; private set; }
    public Currency? Currency { get; private set; }

    public string Name { get; private set; }

    public decimal InitialBalance { get; private set; }

    public string? Color { get; private set; }

    public string? Icon { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    public static Account Create(
        Guid userId,
        Guid currencyId,
        string name,
        decimal initialBalance = 0m,
        string? color = null,
        string? icon = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException(AccountDomainErrors.UserIdRequired);

        if (currencyId == Guid.Empty)
            throw new DomainException(AccountDomainErrors.CurrencyIdRequired);

        var account = new Account
        {
            UserId = userId,
            CurrencyId = currencyId,
            InitialBalance = initialBalance,
            IsArchived = false
        };

        account.SetName(name);
        account.SetColor(color);
        account.SetIcon(icon);

        return account;
    }

    public void Update(
        string name,
        Guid currencyId,
        string? color = null,
        string? icon = null)
    {
        if (currencyId == Guid.Empty)
            throw new DomainException(AccountDomainErrors.CurrencyIdRequired);

        SetName(name);
        SetColor(color);
        SetIcon(icon);

        CurrencyId = currencyId;
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Restore()
    {
        IsArchived = false;
    }

    public void ChangeInitialBalance(decimal initialBalance)
    {
        InitialBalance = initialBalance;
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

    private void SetIcon(string? icon)
    {
        Icon = string.IsNullOrWhiteSpace(icon)
            ? null
            : icon.Trim();
    }
}