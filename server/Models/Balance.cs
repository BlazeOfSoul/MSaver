namespace server.Models;

public class Balance
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal ValueTotal { get; set; }

    public User User { get; set; } = null!;
}
