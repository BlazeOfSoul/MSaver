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
    private const int MaxActiveSessionsPerUser = 3;

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

        await _refreshTokenRepository.DeleteExpiredByUserAsync(user.Id, cancellationToken);

        var existingTokens = await _refreshTokenRepository.GetAsync(user.Id, cancellationToken);

        var activeSessions = existingTokens
            .Where(x => !x.IsExpired)
            .OrderBy(x => x.CreatedAt)
            .ToList();

        while (activeSessions.Count >= MaxActiveSessionsPerUser)
        {
            var oldestSession = activeSessions[0];
            await _refreshTokenRepository.DeleteAsync(oldestSession, cancellationToken);
            activeSessions.RemoveAt(0);
        }

        var clientId = Guid.NewGuid().ToString("N");

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Name,
            user.Email,
            clientId);

        var (refreshTokenValue, refreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
            user.Id,
            user.Name,
            user.Email,
            clientId);

        var refreshEntity = RefreshToken.Create(
            user.Id,
            clientId,
            refreshTokenValue,
            refreshExpiresAt);

        await _refreshTokenRepository.AddAsync(refreshEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            user.Id,
            clientId,
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

        var user = CreateUser(request);

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
        {
            await _refreshTokenRepository.DeleteAsync(storedToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<RefreshTokenResponse>.Failure(AuthDomainErrors.RefreshTokenExpired);
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshTokenResponse>.Failure(AuthDomainErrors.InvalidEmail);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Name,
            user.Email,
            storedToken.ClientId);

        var (newRefreshTokenValue, newRefreshExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken(
            user.Id,
            user.Name,
            user.Email,
            storedToken.ClientId);

        storedToken.Replace(newRefreshTokenValue, newRefreshExpiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(
            user.Id,
            user.Name,
            user.Email,
            storedToken.ClientId,
            accessToken,
            newRefreshTokenValue);

        return Result<RefreshTokenResponse>.Success(response);
    }

    public async Task<Result> LogoutClientAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var session = await _refreshTokenRepository
            .GetByClientIdAsync(userId, clientId, cancellationToken);

        if (session is null)
            return Result.Success();

        await _refreshTokenRepository.DeleteAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LogoutAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _refreshTokenRepository.GetAsync(userId, cancellationToken);

        foreach (var session in sessions)
            await _refreshTokenRepository.DeleteAsync(session, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private User CreateUser(RegisterRequest request)
    {
        var tempUser = User.Create(
            name: request.Username,
            email: request.Email,
            passwordHash: "temp");

        var hash = _passwordHasher.HashPassword(tempUser, request.Password);

        return User.Create(request.Username, request.Email, hash);
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
}