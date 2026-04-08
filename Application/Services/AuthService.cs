using Microsoft.AspNetCore.Identity;

using MSaver.Application.Features.Auth.Login;
using MSaver.Application.Features.Auth.Refresh;
using MSaver.Application.Features.Auth.Register;
using MSaver.Domain.Constants;

namespace MSaver.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    ICategoryRepository categoryRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher) : IAuthService
{
    private const int MaxActiveRefreshTokensPerUser = 3;

    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result<LoginResponse>.Failure(AuthDomainErrors.InvalidEmail);

        var verification = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Result<LoginResponse>.Failure(AuthDomainErrors.InvalidPassword);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Name,
            user.Email);

        var (refreshTokenValue, refreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
            user.Id,
            user.Name,
            user.Email);

        await AddRefreshTokenAsync(user.Id, refreshTokenValue, refreshExpiresAt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            user.Id,
            accessToken,
            refreshTokenValue);

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<Guid>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
            return Result<Guid>.Failure(AuthDomainErrors.RepeatedEmail);

        User user;
        try
        {
            user = CreateUser(request);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await CreateDefaultCategoriesAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
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
            user.Name,
            user.Email);

        var (newRefreshTokenValue, newRefreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
            user.Id,
            user.Name,
            user.Email);

        await AddRefreshTokenAsync(user.Id, newRefreshTokenValue, newRefreshExpiresAt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(
            user.Id,
            user.Name,
            user.Email,
            accessToken,
            newRefreshTokenValue);

        return Result<RefreshTokenResponse>.Success(response);
    }

    private User CreateUser(RegisterRequest request)
    {
        var tempUser = User.Create(
            name: request.Username,
            email: request.Email,
            passwordHash: "temp");

        var hash = _passwordHasher.HashPassword(tempUser, request.Password);

        var user = User.Create(request.Username, request.Email, hash);
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

    private async Task AddRefreshTokenAsync(
        Guid userId,
        string tokenValue,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        var existingTokens = await _refreshTokenRepository
            .GetAsync(userId, cancellationToken);

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