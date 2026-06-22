namespace MSaver.Application.Features.Transactions.Transfer;

public sealed record GetTransferRateRequest(
    Guid FromAccountId,
    Guid ToAccountId);
