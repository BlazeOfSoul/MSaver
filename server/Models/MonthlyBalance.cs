namespace server.Models;

public class MonthlyBalance
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal Balance { get; set; }

    public User User { get; set; } = null!;
}
