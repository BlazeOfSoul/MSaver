namespace MSaver.Domain.Entities;

public sealed class Currency : Entity
{
    private readonly List<Account> _accounts = [];
    private readonly List<Transaction> _transactions = [];
    private readonly List<Transaction> _baseCurrencyTransactions = [];

    private Currency() { }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Symbol { get; private set; }

    public short Precision { get; private set; }

    public bool IsDefault { get; private set; }

    public IReadOnlyCollection<Account> Accounts => _accounts;

    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    public IReadOnlyCollection<Transaction> BaseCurrencyTransactions => _baseCurrencyTransactions;

    public static Currency Create(
        string code,
        string name,
        string symbol,
        short precision,
        bool isDefault = false)
    {
        return new Currency
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Symbol = symbol.Trim(),
            Precision = precision,
            IsDefault = isDefault
        };
    }

    public void SetDefault()
    {
        IsDefault = true;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
    }
}