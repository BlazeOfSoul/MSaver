using Microsoft.AspNetCore.Mvc;
using server.Api.Common;
using server.Api.Extensions;
using server.Application.Services.Interfaces;

namespace server.Api.Controllers;

[Route("api/[controller]")]
public sealed class BalanceController : ApiControllerBase
{
    private readonly IBalanceService _balanceService;

    public BalanceController(IBalanceService balanceService)
    {
        _balanceService = balanceService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _balanceService.GetCurrentAsync(userId, cancellationToken);
        return FromResult(result);
    }
}
