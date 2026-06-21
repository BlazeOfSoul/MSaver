namespace MSaver.Application.Features.Transactions.Transfer;

public sealed record GetTransferRateResponse(
    decimal Rate,
    string FromCurrencyCode,
    string ToCurrencyCode);
