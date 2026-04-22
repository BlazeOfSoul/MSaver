namespace MSaver.Application.Features.Transactions.Transfer;

public sealed record CreateTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime Date,
    decimal? Rate,
    string? Description);