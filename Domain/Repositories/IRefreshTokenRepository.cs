namespace MSaver.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
