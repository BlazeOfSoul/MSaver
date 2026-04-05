namespace MSaver.Application.Features.Transactions.Get;

public sealed class GetTransactionsResponse
{
    public IReadOnlyCollection<TransactionItemResponse> Items
    {
        get; init;
    } = Array.Empty<TransactionItemResponse>();
}