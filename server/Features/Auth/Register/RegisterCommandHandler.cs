using MediatR;

using Microsoft.AspNetCore.Identity;

using server.Data;
using server.Domain.Interfaces;
using server.Models;
using server.Models.Constants;
using server.Repositories.Interfaces;

namespace server.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBalanceRepository _balanceRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        ApplicationDbContext dbContext,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IBalanceRepository balanceRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _balanceRepository = balanceRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception(ErrorMessages.Auth.RepeatedEmail);

            var user = CreateUser(request);
            await _userRepository.AddAsync(user);

            await CreateDefaultCategoriesAsync(user.Id, cancellationToken);
            await CreateInitialBalanceAsync(user.Id, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);
            return new RegisterResponse(user.Id, user.Username, user.Email, token);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private User CreateUser(RegisterCommand request)
    {
        var passwordHasher = new PasswordHasher<User>();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        return user;
    }

    private async Task CreateDefaultCategoriesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var defaultCategories = DefaultCategories.Map.Select(kv =>
        {
            var (name, type, color) = kv.Value;
            return new Category
            {
                UserId = userId,
                Name = name,
                Type = type,
                Color = color,
                IsDeleted = false
            };
        }).ToList();

        await _categoryRepository.AddRangeAsync(defaultCategories, cancellationToken);
    }

    private async Task CreateInitialBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var balance = new Models.Balance
        {
            UserId = userId,
            Year = now.Year,
            Month = now.Month,
            IncomeTotal = 0,
            ExpenseTotal = 0,
            ValueTotal = 0
        };

        await _balanceRepository.AddAsync(balance, cancellationToken);
    }
}
