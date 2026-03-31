using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Entities;
using MSaver.Domain.Repositories;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<RefreshToken>().AddAsync(token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<RefreshToken>()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.Revoke();
        _dbContext.Set<RefreshToken>().Update(token);
        return Task.CompletedTask;
    }
}
