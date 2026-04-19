using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Api.Contracts.Tags;
using MSaver.Application.Features.Tags.AssignCategories;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TagsController(
    ITagService tagService,
    IValidator<CreateTagRequest> createValidator,
    IValidator<UpdateTagRequest> updateValidator,
    IValidator<AssignTagCategoriesRequest> assignTagCategoriesValidator) : ApiControllerBase
{
    private readonly ITagService _tagService = tagService;
    private readonly IValidator<CreateTagRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateTagRequest> _updateValidator = updateValidator;
    private readonly IValidator<AssignTagCategoriesRequest> _assignTagCategoriesValidator = assignTagCategoriesValidator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _tagService.GetAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _createValidator,
            ct => _tagService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTagBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateTagRequest(
            id,
            body.Name,
            body.Color);

        return ValidateAndExecuteAsync(
            request,
            _updateValidator,
            ct => _tagService.UpdateAsync(request, ct),
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _tagService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPut("{tagId:guid}/categories")]
    public Task<IActionResult> AssignCategories(
        Guid tagId,
        [FromBody] AssignTagCategoriesBody body,
        CancellationToken cancellationToken)
    {
        var request = new AssignTagCategoriesRequest
        {
            TagId = tagId,
            CategoryIds = body.CategoryIds
        };

        return ValidateAndExecuteAsync(
            request,
            _assignTagCategoriesValidator,
            ct => _tagService.AssignCategoriesAsync(request, ct),
            cancellationToken);
    }
}