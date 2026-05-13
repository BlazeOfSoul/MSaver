namespace MSaver.Api.Contracts.Transactions;

public sealed record UpdateTransactionBody(
    Guid CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description);