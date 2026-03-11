using server.Domain.Common;

namespace server.Domain.Errors;

public static class ExchangeRateDomainErrors
{
    public static readonly DomainError NbrbRateNotFound =
        new(
            DomainErrorType.Failure,
            "ExchangeRate.NbrbRateNotFound",
            "Не удалось получить данные курсов НБРБ.",
            null);

    public static readonly DomainError CoinGeckoNotFound =
        new(
            DomainErrorType.Failure,
            "ExchangeRate.CoinGeckoNotFound",
            "Не удалось получить данные с CoinGecko.",
            null);
}
