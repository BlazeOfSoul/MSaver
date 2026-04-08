using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController(
    ICategoryService categoryService,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ApiControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IValidator<CreateCategoryRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator = updateValidator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync<CreateCategoryRequest, Guid>(
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
        var result = await _categoryService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}