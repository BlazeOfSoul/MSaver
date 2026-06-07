using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Application.Features.Users.UpdateApplicationCurrency;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class UsersController(
    IUserService userService) : ApiControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _userService.GetCurrentAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpPatch("me/application-currency")]
    public async Task<IActionResult> UpdateApplicationCurrency(
        UpdateApplicationCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateApplicationCurrencyAsync(request, cancellationToken);
        return FromResult(result);
    }
}
