using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using server.Api.Common;
using server.Application.Abstractions.Auth;
using server.Application.Abstractions.Services;
using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;

namespace server.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUser;

    public CategoriesController(
        ICategoryService categoryService,
        ICurrentUserService currentUser)
    {
        _categoryService = categoryService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCategories(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var request = new GetCategoriesRequest(userId);

        var result = await _categoryService.GetCategoriesAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = _currentUser.UserId;

        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

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
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var command = new DeleteCategoryRequest(id, userId);

        var result = await _categoryService.DeleteCategoryAsync(command, cancellationToken);
        return FromResult(result);
    }
}