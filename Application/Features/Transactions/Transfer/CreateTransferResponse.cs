namespace MSaver.Application.Features.Transactions.Transfer;

public sealed record CreateTransferResponse(
    Guid TransferId,
    Guid ExpenseTransactionId,
    Guid IncomeTransactionId,
    decimal WithdrawAmount,
    decimal DepositAmount,
    decimal Rate,
    string FromCurrencyCode,
    string ToCurrencyCode);
