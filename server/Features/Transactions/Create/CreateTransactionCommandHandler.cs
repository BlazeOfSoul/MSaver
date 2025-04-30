using MediatR;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Models.Enums;
using server.Data;

namespace server.Features.Transactions.Create;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateTransactionCommandHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId, cancellationToken);

        if (category == null)
            throw new Exception("Категория не найдена");

        var now = request.Date;
        var balance = await _dbContext.Balances
            .FirstOrDefaultAsync(mb =>
                mb.UserId == request.UserId &&
                mb.Year == now.Year &&
                mb.Month == now.Month,
                cancellationToken);

        if (balance == null)
        {
            balance = new Models.Balance
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Year = now.Year,
                Month = now.Month,
                IncomeTotal = 0,
                ExpenseTotal = 0,
                ValueTotal = 0
            };
            _dbContext.Balances.Add(balance);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            Amount = request.Amount,
            Date = request.Date
        };

        _dbContext.Transactions.Add(transaction);

        if (category.Type == CategoryType.Income)
        {
            balance.IncomeTotal += request.Amount;
        }
        else
        {
            balance.ExpenseTotal += request.Amount;
        }

        balance.ValueTotal = balance.IncomeTotal - balance.ExpenseTotal;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }

}
