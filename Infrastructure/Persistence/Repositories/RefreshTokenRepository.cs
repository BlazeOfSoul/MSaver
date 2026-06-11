namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.AddAsync(token, cancellationToken);

    public async Task<IEnumerable<RefreshToken>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.RefreshTokens
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshToken.HashToken(token);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == tokenHash, cancellationToken);

        if (storedToken is not null || LooksLikeSha256Hash(token))
            return storedToken;

        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task<RefreshToken?> GetByClientIdAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default)
        => await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.ClientId == clientId,
                cancellationToken);

    public Task DeleteAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Remove(token);
        return Task.CompletedTask;
    }

    public async Task DeleteExpiredByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.ExpiresAt <= utcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static bool LooksLikeSha256Hash(string token)
    {
        if (token.Length != 64)
            return false;

        return token.All(Uri.IsHexDigit);
    }
}
