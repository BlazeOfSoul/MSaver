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
        var monthlyBalance = await _dbContext.MonthlyBalances
            .FirstOrDefaultAsync(mb =>
                mb.UserId == request.UserId &&
                mb.Year == now.Year &&
                mb.Month == now.Month,
                cancellationToken);

        if (monthlyBalance == null)
        {
            // Создаем, если нет
            monthlyBalance = new MonthlyBalance
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Year = now.Year,
                Month = now.Month,
                IncomeTotal = 0,
                ExpenseTotal = 0,
                Balance = 0
            };
            _dbContext.MonthlyBalances.Add(monthlyBalance);
        }

        // Создаём транзакцию
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

        // Обновляем месячный баланс
        if (category.Type == CategoryType.Income)
        {
            monthlyBalance.IncomeTotal += request.Amount;
        }
        else
        {
            monthlyBalance.ExpenseTotal += request.Amount;
        }

        monthlyBalance.Balance = monthlyBalance.IncomeTotal - monthlyBalance.ExpenseTotal;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }

}
