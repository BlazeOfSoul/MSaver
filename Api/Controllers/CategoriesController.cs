using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Abstractions.Auth;
using MSaver.Application.Abstractions.Services;
using MSaver.Application.Features.Categories.CreateCategory;
using MSaver.Application.Features.Categories.DeleteCategory;
using MSaver.Application.Features.Categories.GetCategories;
using MSaver.Application.Features.Categories.UpdateCategory;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateCategoryRequest> _createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator;

    public CategoriesController(
        ICategoryService categoryService,
        ICurrentUserService currentUser,
        IValidator<CreateCategoryRequest> createValidator,
        IValidator<UpdateCategoryRequest> updateValidator)
    {
        _categoryService = categoryService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
    public Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = _currentUser.UserId;

        return ValidateAndExecuteAsync<CreateCategoryRequest, CreateCategoryResponse>(
            request,
            _createValidator,
            ct => _categoryService.CreateCategoryAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> UpdateCategory(
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

        return ValidateAndExecuteAsync(
            command,
            _updateValidator,
            ct => _categoryService.UpdateCategoryAsync(command, ct),
            cancellationToken);
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