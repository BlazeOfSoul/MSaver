using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using server.Extensions;
using server.Features.Categories.GetCategories;
using System.Security.Claims;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCategories()
    {
        var userId = User.GetUserId();
        var query = new GetCategoriesQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
