using Microsoft.AspNetCore.Identity;

using server.Application.Common.Results;
using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Refresh;
using server.Application.Features.Auth.Register;
using server.Application.Services.Interfaces;
using server.Application.Abstractions.Services;
using server.Domain.Common;
using server.Domain.Entities;
using server.Domain.Errors;
using server.Domain.Repositories;
using server.Domain.Constants;

namespace server.Application.Services;

public sealed class AuthService : IAuthService
{
    private const int MaxActiveRefreshTokensPerUser = 3;

    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBalanceRepository _balanceRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IBalanceRepository balanceRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _balanceRepository = balanceRepository;
        _refreshTokenRepository = refreshTokenRepository;
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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Username,
            user.Email);

        var (refreshTokenValue, refreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
            user.Id,
            user.Username,
            user.Email);

        await AddRefreshTokenAsync(user.Id, refreshTokenValue, refreshExpiresAt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            refreshTokenValue);

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null)
                return Result<RegisterResponse>.Failure(AuthDomainErrors.RepeatedEmail);

            var user = CreateUser(request);
            await _userRepository.AddAsync(user, cancellationToken);

            await CreateDefaultCategoriesAsync(user.Id, cancellationToken);
            await CreateInitialBalanceAsync(user.Id, cancellationToken);

            var accessToken = _jwtTokenGenerator.GenerateAccessToken(
                user.Id,
                user.Username,
                user.Email);

            var (refreshTokenValue, refreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
                user.Id,
                user.Username,
                user.Email);

            await AddRefreshTokenAsync(user.Id, refreshTokenValue, refreshExpiresAt, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RegisterResponse(
                user.Id,
                user.Username,
                user.Email,
                accessToken,
                refreshTokenValue);

            return Result<RegisterResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<RegisterResponse>.Failure(ex.Error);
        }
    }

    public async Task<Result<RefreshTokenResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenRepository
            .GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken is null)
            return Result<RefreshTokenResponse>.Failure(AuthDomainErrors.RefreshTokenInvalid);

        if (storedToken.IsExpired)
            return Result<RefreshTokenResponse>.Failure(AuthDomainErrors.RefreshTokenExpired);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshTokenResponse>.Failure(AuthDomainErrors.InvalidEmail);

        await _refreshTokenRepository.RevokeAsync(storedToken, cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Username,
            user.Email);

        var (newRefreshTokenValue, newRefreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
            user.Id,
            user.Username,
            user.Email);

        await AddRefreshTokenAsync(user.Id, newRefreshTokenValue, newRefreshExpiresAt, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            newRefreshTokenValue);

        return Result<RefreshTokenResponse>.Success(response);
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

    private async Task AddRefreshTokenAsync(
        Guid userId,
        string tokenValue,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        var existingTokens = await _refreshTokenRepository
            .GetByUserIdAsync(userId, cancellationToken);

        var activeTokens = existingTokens
            .Where(t => !t.IsExpired)
            .OrderBy(t => t.CreatedAt)
            .ToList();

        while (activeTokens.Count >= MaxActiveRefreshTokensPerUser)
        {
            var oldest = activeTokens[0];
            await _refreshTokenRepository.RevokeAsync(oldest, cancellationToken);
            activeTokens.RemoveAt(0);
        }

        var refreshEntity = RefreshToken.Create(userId, tokenValue, expiresAt);
        await _refreshTokenRepository.AddAsync(refreshEntity, cancellationToken);
    }
}
