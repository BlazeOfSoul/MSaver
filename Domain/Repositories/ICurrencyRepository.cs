namespace MSaver.Domain.Repositories;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Currency> GetDefaultCurrencyAsync(
        CancellationToken cancellationToken = default);
}