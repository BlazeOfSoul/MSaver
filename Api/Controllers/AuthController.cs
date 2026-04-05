using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Auth.Login;
using MSaver.Application.Features.Auth.Refresh;
using MSaver.Application.Features.Auth.Register;

namespace MSaver.Api.Controllers;

[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshValidator) : ApiControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IValidator<RegisterRequest> _registerValidator = registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator = loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator = refreshValidator;

    [HttpPost("register")]
    [AllowAnonymous]
    public Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        => ValidateAndExecuteAsync<RegisterRequest, RegisterResponse>(
            request,
            _registerValidator,
            ct => _authService.RegisterAsync(request, ct),
            cancellationToken);

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
        => ValidateAndExecuteAsync<LoginRequest, LoginResponse>(
            request,
            _loginValidator,
            ct => _authService.LoginAsync(request, ct),
            cancellationToken);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
        => ValidateAndExecuteAsync<RefreshTokenRequest, RefreshTokenResponse>(
            request,
            _refreshValidator,
            ct => _authService.RefreshAsync(request, ct),
            cancellationToken);
}