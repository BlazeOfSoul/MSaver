using MediatR;
using Microsoft.AspNetCore.Mvc;

using server.Extensions;
using server.Features.Categories.CreateCategory;
using server.Features.Categories.DeleteCategory;
using server.Features.Categories.GetCategories;
using server.Features.Categories.UpdateCategory;

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
    
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        command.UserId = User.GetUserId();

        var createdCategory = await _mediator.Send(command);
        return Ok(createdCategory);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var userId = User.GetUserId();
        var command = new UpdateCategoryCommand(id, userId, request.Name, request.Color, request.Type);
        var success = await _mediator.Send(command);

        return success ? NoContent() : NotFound();
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var userId = User.GetUserId();
        var command = new DeleteCategoryCommand(id, userId);
        var success = await _mediator.Send(command);

        return success ? NoContent() : NotFound();
    }
}
