using Microsoft.EntityFrameworkCore;

using MSaver.Domain.Entities;
using MSaver.Domain.Repositories;
using MSaver.Infrastructure.Persistence;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class BalanceRepository : IBalanceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BalanceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Balance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.Balances.AddAsync(balance, cancellationToken);
    }

    public async Task<Balance?> GetByUserAndDateAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Balances
            .FirstOrDefaultAsync(
                b => b.UserId == userId &&
                     b.Year == year &&
                     b.Month == month,
                cancellationToken);
    }

    public Task UpdateAsync(Balance balance, CancellationToken cancellationToken = default)
    {
        _dbContext.Balances.Update(balance);
        return Task.CompletedTask;
    }

    public async Task<Balance?> GetCurrentByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Balances
            .FirstOrDefaultAsync(
                m => m.UserId == userId &&
                     m.Year == now.Year &&
                     m.Month == now.Month,
                cancellationToken);
    }
}
