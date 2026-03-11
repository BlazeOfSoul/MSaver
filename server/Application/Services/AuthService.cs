using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using server.Application.Common.Results;
using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Register;
using server.Application.Services.Interfaces;
using server.Application.Abstractions.Services;
using server.Domain.Common;
using server.Domain.Entities;
using server.Domain.Errors;
using server.Domain.Repositories;
using server.Infrastructure.Persistence;
using server.Domain.Constants;

namespace server.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBalanceRepository _balanceRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        ApplicationDbContext dbContext,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IBalanceRepository balanceRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _balanceRepository = balanceRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result<LoginResponse>.Failure(AuthDomainErrors.InvalidEmail);
        }

        var passwordHasher = new PasswordHasher<User>();
        var verification = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Result<LoginResponse>.Failure(AuthDomainErrors.InvalidPassword);
        }

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);
        var response = new LoginResponse(user.Id, user.Username, user.Email, token);

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null)
                return Result<RegisterResponse>.Failure(AuthDomainErrors.RepeatedEmail);

            var user = CreateUser(request);
            await _userRepository.AddAsync(user, cancellationToken);

            await CreateDefaultCategoriesAsync(user.Id, cancellationToken);
            await CreateInitialBalanceAsync(user.Id, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);
            var response = new RegisterResponse(user.Id, user.Username, user.Email, token);

            return Result<RegisterResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<RegisterResponse>.Failure(ex.Error);
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

        var user = User.Create(request.Username, request.Email, string.Empty);

        var hash = passwordHasher.HashPassword(user, request.Password);
        user.ChangePassword(hash);

        return user;
    }

    private async Task CreateDefaultCategoriesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var defaultCategories = DefaultCategories.Map
            .Select(kv =>
            {
                var (name, type, color) = kv.Value;
                return Category.Create(userId, name, type, color);
            })
            .ToList();

        await _categoryRepository.AddRangeAsync(defaultCategories, cancellationToken);
    }

    private async Task CreateInitialBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var balance = Balance.Create(userId, now.Year, now.Month);
        await _balanceRepository.AddAsync(balance, cancellationToken);
    }
}
