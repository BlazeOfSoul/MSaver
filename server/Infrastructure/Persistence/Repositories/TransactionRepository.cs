using Microsoft.EntityFrameworkCore;
using server.Domain.Entities;
using server.Domain.Repositories;

namespace server.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdWithCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
