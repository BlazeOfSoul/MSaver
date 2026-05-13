namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionItemResponse
{
    public Guid Id { get; init; }

    public TransactionAccountResponse Account { get; init; } = null!;

    public TransactionCategoryResponse Category { get; init; } = null!;

    public decimal Amount { get; init; }

    public DateTime Date { get; init; }

    public string Description { get; init; } = string.Empty;
}