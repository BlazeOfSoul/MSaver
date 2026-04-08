using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;

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
}