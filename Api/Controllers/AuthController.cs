using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Auth;
using MSaver.Api.Common;
using MSaver.Application.Features.Auth.Login;
using MSaver.Application.Features.Auth.Refresh;
using MSaver.Application.Features.Auth.Register;

namespace MSaver.Api.Controllers;

[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshValidator,
    AuthCookieService authCookieService) : ApiControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IValidator<RegisterRequest> _registerValidator = registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator = loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator = refreshValidator;
    private readonly AuthCookieService _authCookieService = authCookieService;

    [HttpPost("register")]
    [AllowAnonymous]
    public Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        => ValidateAndExecuteAsync(
            request,
            _registerValidator,
            ct => _authService.RegisterAsync(request, ct),
            cancellationToken);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(ApiErrorFactory.ValidationFailed(validationResult));

        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result.IsFailure)
            return FromResult(result);

        var session = result.Value!;
        _authCookieService.AppendAuthCookies(
            Request,
            Response,
            session.AccessToken,
            session.RefreshToken);

        return Ok(AuthSessionResponse.From(session));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = _authCookieService.ReadRefreshToken(Request);

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(ApiErrorFactory.Unauthorized());

        var request = new RefreshTokenRequest(refreshToken);
        var validationResult = await _refreshValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(ApiErrorFactory.ValidationFailed(validationResult));

        var result = await _authService.RefreshAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            _authCookieService.ClearAuthCookies(Request, Response);
            return FromResult(result);
        }

        var session = result.Value!;
        _authCookieService.AppendAuthCookies(
            Request,
            Response,
            session.AccessToken,
            session.RefreshToken);

        return Ok(AuthSessionResponse.From(session));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutClientAsync(
            _currentUserService.UserId,
            _currentUserService.ClientId,
            cancellationToken);

        _authCookieService.ClearAuthCookies(Request, Response);

        return FromResult(result);
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAllAsync(
            _currentUserService.UserId,
            cancellationToken);

        _authCookieService.ClearAuthCookies(Request, Response);

        return FromResult(result);
    }
}

public sealed record AuthSessionResponse(
    Guid Id,
    string Name,
    string Email,
    string ClientId)
{
    public static AuthSessionResponse From(LoginResponse response) =>
        new(response.Id, response.Name, response.Email, response.ClientId);

    public static AuthSessionResponse From(RefreshTokenResponse response) =>
        new(response.Id, response.Name, response.Email, response.ClientId);
}
