namespace MSaver.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByClientIdAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task DeleteExpiredByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}