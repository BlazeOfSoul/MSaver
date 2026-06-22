namespace MSaver.Application.Features.Transactions.Get;

public sealed record TransactionPeriodBreakdown
{
    public TransactionPeriodBreakdown(
        decimal Income,
        decimal Expense,
        decimal TransferIn,
        decimal TransferOut)
    {
        this.Income = Math.Abs(Income);
        this.Expense = Math.Abs(Expense);
        this.TransferIn = Math.Abs(TransferIn);
        this.TransferOut = Math.Abs(TransferOut);
    }

    public decimal Income { get; }
    public decimal Expense { get; }
    public decimal TransferIn { get; }
    public decimal TransferOut { get; }

    public decimal OperationsChange => Income - Expense;

    public decimal TransferChange => TransferIn - TransferOut;
}
