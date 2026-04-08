namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionTagResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Color { get; init; }
}