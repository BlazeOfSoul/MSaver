using server.Domain.Common;
using server.Domain.Errors;

namespace server.Domain.Entities;

public sealed class Balance : Entity
{
    private Balance() { }

    public Guid UserId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }

    public decimal IncomeTotal { get; private set; }
    public decimal ExpenseTotal { get; private set; }
    public decimal ValueTotal { get; private set; }

    public User User { get; private set; } = null!;

    public Balance(Guid userId, int year, int month)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(DomainErrors.Balance.UserIdRequired, nameof(userId));

        if (year < 2000 || year > 3000)
            throw new ArgumentOutOfRangeException(nameof(year), DomainErrors.Balance.InvalidYear);

        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), DomainErrors.Balance.InvalidMonth);

        UserId = userId;
        Year = year;
        Month = month;
        IncomeTotal = 0;
        ExpenseTotal = 0;
        ValueTotal = 0;
    }

    public void ApplyIncome(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), DomainErrors.Balance.NegativeAmount);

        IncomeTotal += amount;
        RecalculateValue();
    }

    public void ApplyExpense(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), DomainErrors.Balance.NegativeAmount);

        ExpenseTotal += amount;
        RecalculateValue();
    }

    private void RecalculateValue()
    {
        ValueTotal = IncomeTotal - ExpenseTotal;
    }
}
