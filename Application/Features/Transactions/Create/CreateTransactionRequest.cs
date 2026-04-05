namespace MSaver.Application.Features.Transactions.Create;

public sealed class CreateTransactionRequest
{
    public Guid UserId { get; set; }

    public Guid AccountId { get; set; }

    public Guid CategoryId { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public IReadOnlyCollection<Guid> TagIds { get; set; } = [];
}