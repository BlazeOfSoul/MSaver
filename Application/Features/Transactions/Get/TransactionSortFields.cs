namespace MSaver.Application.Features.Transactions.Get;

public static class TransactionSortFields
{
    public const string Date = "date";
    public const string Amount = "amount";

    public static readonly string[] All =
    [
        Date,
        Amount
    ];
}