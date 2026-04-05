namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionItemResponse
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public Guid CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string CategoryColor { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public DateTime Date { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyCollection<Guid> TagIds { get; init; } = [];

    public IReadOnlyCollection<string> Tags { get; init; } = [];

    public bool IsTransfer { get; init; }
}