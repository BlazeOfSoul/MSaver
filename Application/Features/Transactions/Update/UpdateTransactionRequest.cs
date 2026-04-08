namespace MSaver.Application.Features.Transactions.Update;

public sealed record UpdateTransactionRequest(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    decimal Amount,
    DateTime Date,
    string Description,
    IReadOnlyCollection<Guid> TagIds);