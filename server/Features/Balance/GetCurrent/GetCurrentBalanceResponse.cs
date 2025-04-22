namespace server.Features.Balance.GetCurrent;

public record GetCurrentBalanceResponse(decimal IncomeTotal, decimal ExpenseTotal, decimal Balance);
