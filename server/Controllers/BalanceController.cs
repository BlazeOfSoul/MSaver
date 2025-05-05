using MediatR;
using Microsoft.AspNetCore.Mvc;

using server.Extensions;
using server.Features.Balance.GetCurrent;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public BalanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetCurrentBalanceQuery(userId));
        return Ok(result);
    }

}
