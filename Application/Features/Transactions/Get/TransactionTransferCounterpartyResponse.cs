namespace MSaver.Application.Features.Transactions.Get;

public sealed class TransactionTransferCounterpartyResponse
{
    public Guid Id { get; init; }

    public TransactionAccountResponse Account { get; init; } = null!;

    public decimal Amount { get; init; }
}
