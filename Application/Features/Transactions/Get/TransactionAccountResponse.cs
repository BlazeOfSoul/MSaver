namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionAccountResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Color { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;

    public bool IsArchived { get; init; }
}