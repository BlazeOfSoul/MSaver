namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class CurrencyRepository(ApplicationDbContext context) : ICurrencyRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Currency?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Currency> GetDefaultCurrencyAsync(
        CancellationToken cancellationToken = default)
    {
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(x => x.IsDefault, cancellationToken);

        if (currency is null)
            throw new DomainException(CurrencyDomainErrors.DefaultCurrencyNotFound);

        return currency;
    }
}