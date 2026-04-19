namespace MSaver.Application.Features.Transactions.Update;

public sealed record UpdateTransactionRequest(
    Guid Id,
    Guid CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description);