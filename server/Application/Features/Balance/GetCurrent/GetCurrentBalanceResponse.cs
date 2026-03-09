namespace server.Application.Features.Balance.GetCurrent;

public sealed record GetCurrentBalanceResponse(
    decimal IncomeTotal,
    decimal ExpenseTotal,
    decimal Balance);