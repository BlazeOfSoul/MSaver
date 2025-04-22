using Microsoft.EntityFrameworkCore;

using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class MonthlyBalanceRepository : IMonthlyBalanceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MonthlyBalanceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MonthlyBalance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.MonthlyBalances.AddAsync(balance, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MonthlyBalance?> GetByUserAndDateAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MonthlyBalances
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Year == year && b.Month == month, cancellationToken);
    }

    public async Task UpdateAsync(MonthlyBalance balance, CancellationToken cancellationToken = default)
    {
        _dbContext.MonthlyBalances.Update(balance);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MonthlyBalance?> GetCurrentByUserIdAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.MonthlyBalances
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Year == now.Year && m.Month == now.Month);
    }

}
