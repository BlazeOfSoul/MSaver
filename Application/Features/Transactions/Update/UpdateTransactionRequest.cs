namespace MSaver.Application.Features.Transactions.Update;

public sealed class UpdateTransactionRequest
{
    public Guid Id
    {
        get; set;
    }

    public Guid UserId
    {
        get; set;
    }

    public Guid AccountId
    {
        get; set;
    }

    public Guid CategoryId
    {
        get; set;
    }

    public decimal Amount
    {
        get; set;
    }

    public DateTime Date
    {
        get; set;
    }

    public string Description
    {
        get; set;
    } = string.Empty;

    public IReadOnlyCollection<Guid> TagIds
    {
        get; set;
    } = Array.Empty<Guid>();
}