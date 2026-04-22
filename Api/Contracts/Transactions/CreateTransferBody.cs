namespace MSaver.Api.Contracts.Transactions;

public sealed record CreateTransferBody(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime Date,
    decimal? Rate,
    string? Description);