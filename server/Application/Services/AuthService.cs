using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using server.Application.Abstractions.Repositories;
using server.Application.Constants;
using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Register;
using server.Application.Services.Interfaces;
using server.Domain.Constants;
using server.Domain.Entities;
using server.Domain.Enums;
using server.Application.Abstractions.Services;
using server.Infrastructure.Persistence;

namespace server.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBalanceRepository _balanceRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
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

    public async Task<LoginResponse> LoginAsync(LoginRequest query, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(query.Email, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException(ErrorMessages.Auth.InvalidEmail);
        }

        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, query.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException(ErrorMessages.Auth.InvalidPassword);
        }

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);

        return new LoginResponse(user.Id, user.Username, user.Email, token);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser is not null)
            {
                throw new InvalidOperationException(ErrorMessages.Auth.RepeatedEmail);
            }

            var user = CreateUser(command);
            await _userRepository.AddAsync(user, cancellationToken);

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

    private static User CreateUser(RegisterRequest request)
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

        var balance = new Balance
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
