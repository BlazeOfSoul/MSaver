namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionCategoryResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Color { get; init; } = string.Empty;
}