using Microsoft.AspNetCore.Mvc;
using server.Application.Services.Interfaces;
using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Register;

namespace server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest command, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest query, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(query, cancellationToken);
        return Ok(response);
    }
}
