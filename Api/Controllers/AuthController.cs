using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Auth.Login;
using MSaver.Application.Features.Auth.Logout;
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
    IValidator<LogoutClientRequest> logoutValidator) : ApiControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IValidator<RegisterRequest> _registerValidator = registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator = loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator = refreshValidator;
    private readonly IValidator<LogoutClientRequest> _logoutValidator = logoutValidator;

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
    public Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
        => ValidateAndExecuteAsync(
            request,
            _loginValidator,
            ct => _authService.LoginAsync(request, ct),
            cancellationToken);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
        => ValidateAndExecuteAsync(
            request,
            _refreshValidator,
            ct => _authService.RefreshAsync(request, ct),
            cancellationToken);

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutClientRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _logoutValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(ApiErrorFactory.ValidationFailed(validationResult));

        var result = await _authService.LogoutClientAsync(
            _currentUserService.UserId,
            request.ClientId,
            cancellationToken);

        return FromResult(result);
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAllAsync(
            _currentUserService.UserId,
            cancellationToken);

        return FromResult(result);
    }
}