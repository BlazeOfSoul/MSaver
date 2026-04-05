using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Delete;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController(
    ICategoryService categoryService,
    ICurrentUserService currentUser,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ApiControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IValidator<CreateCategoryRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator = updateValidator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var request = new GetCategoriesRequest(_currentUser.UserId);

        var result = await _categoryService.GetAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = _currentUser.UserId;

        return ValidateAndExecuteAsync<CreateCategoryRequest, CreateCategoryResponse>(
            request,
            _createValidator,
            ct => _categoryService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryRequest(
            id,
            _currentUser.UserId,
            request.Name,
            request.Color,
            request.Type);

        return ValidateAndExecuteAsync<UpdateCategoryRequest, Guid>(
            command,
            _updateValidator,
            ct => _categoryService.UpdateAsync(command, ct),
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryRequest(id, _currentUser.UserId);

        var result = await _categoryService.DeleteAsync(command, cancellationToken);
        return FromResult(result);
    }
}