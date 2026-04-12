namespace MSaver.Api.Contracts.Transactions;

public sealed record UpdateTransactionBody(
    Guid AccountId,
    Guid CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description,
    Guid[] TagIds);