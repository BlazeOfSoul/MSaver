namespace MSaver.Application.Features.Transactions.Get;

public sealed record TransactionPeriodBreakdown(
    decimal Income,
    decimal Expense,
    decimal TransferIn,
    decimal TransferOut)
{
    public decimal OperationsChange => Income + Expense;

    public decimal TransferChange => TransferIn + TransferOut;
}
