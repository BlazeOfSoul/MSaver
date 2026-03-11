using Microsoft.AspNetCore.Mvc;

using server.Api.Common;
using server.Api.Extensions;
using server.Application.Services.Interfaces;
using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;
using Microsoft.AspNetCore.Authorization;

namespace server.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController : ApiControllerBase
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
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = User.GetUserId();

        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return FromResult(result);
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

        var result = await _categoryService.UpdateCategoryAsync(command, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var command = new DeleteCategoryRequest(id, userId);

        var result = await _categoryService.DeleteCategoryAsync(command, cancellationToken);
        return FromResult(result);
    }
}
