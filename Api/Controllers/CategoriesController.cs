using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Contracts.Categories;
using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController(
    ICategoryService categoryService,
    IValidator<GetCategoriesRequest> getValidator,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ApiControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IValidator<GetCategoriesRequest> _getValidator = getValidator;
    private readonly IValidator<CreateCategoryRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator = updateValidator;

    [HttpGet]
    public Task<IActionResult> Get(
        [FromQuery] GetCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _getValidator,
            ct => _categoryService.GetAsync(request, ct),
            cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _createValidator,
            ct => _categoryService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateCategoryRequest(
            id,
            body.Name,
            body.Color,
            body.Type);

        return ValidateAndExecuteAsync(
            request,
            _updateValidator,
            ct => _categoryService.UpdateAsync(request, ct),
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