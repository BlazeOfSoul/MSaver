using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Abstractions.Auth;
using MSaver.Application.Abstractions.Services;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class BalanceController : ApiControllerBase
{
    private readonly IBalanceService _balanceService;
    private readonly ICurrentUserService _currentUser;

    public BalanceController(
        IBalanceService balanceService,
        ICurrentUserService currentUser)
    {
        _balanceService = balanceService;
        _currentUser = currentUser;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var result = await _balanceService.GetCurrentAsync(userId, cancellationToken);
        return FromResult(result);
    }
}