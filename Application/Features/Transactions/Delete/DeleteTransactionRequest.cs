namespace MSaver.Application.Features.Transactions.Delete;

public sealed record DeleteTransactionRequest(Guid Id, Guid UserId);