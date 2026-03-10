using Microsoft.AspNetCore.Mvc;
using server.Api.Extensions;
using server.Application.Services.Interfaces;
using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;

namespace server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCategories(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var request = new GetCategoriesRequest(userId);

        var result = await _categoryService.GetCategoriesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateCategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = User.GetUserId();

        var createdCategory = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return Ok(createdCategory);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var command = new UpdateCategoryRequest(
            id,
            userId,
            request.Name,
            request.Color,
            request.Type);

        var success = await _categoryService.UpdateCategoryAsync(command, cancellationToken);

        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var command = new DeleteCategoryRequest(id, userId);

        var success = await _categoryService.DeleteCategoryAsync(command, cancellationToken);

        return success ? NoContent() : NotFound();
    }
}
