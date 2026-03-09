namespace server.Application.Features.Transactions.Create;

public sealed class CreateTransactionCommand
{
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public Guid UserId { get; set; }
}