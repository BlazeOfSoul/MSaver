using MediatR;
using Microsoft.AspNetCore.Mvc;
using MSaver.Api.Features.Auth.Login;
using MSaver.Api.Features.Auth.Register;

namespace MSaver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

}
