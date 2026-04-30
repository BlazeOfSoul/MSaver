using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Contracts.Tags;
using MSaver.Application.Features.Tags.AssignCategories;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TagsController(
    ITagService tagService,
    IValidator<GetTagsRequest> getValidator,
    IValidator<CreateTagRequest> createValidator,
    IValidator<UpdateTagRequest> updateValidator,
    IValidator<AssignTagCategoriesRequest> assignTagCategoriesValidator) : ApiControllerBase
{
    private readonly ITagService _tagService = tagService;
    private readonly IValidator<GetTagsRequest> _getValidator = getValidator;
    private readonly IValidator<CreateTagRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateTagRequest> _updateValidator = updateValidator;
    private readonly IValidator<AssignTagCategoriesRequest> _assignTagCategoriesValidator = assignTagCategoriesValidator;

    [HttpGet]
    public Task<IActionResult> Get(
        [FromQuery] GetTagsRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _getValidator,
            cancellationToken => _tagService.GetAsync(request, cancellationToken),
            cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _tagService.GetByIdAsync(id, cancellationToken);
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
            cancellationToken => _tagService.CreateAsync(request, cancellationToken),
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
            cancellationToken => _tagService.UpdateAsync(request, cancellationToken),
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
            cancellationToken => _tagService.AssignCategoriesAsync(request, cancellationToken),
            cancellationToken);
    }
}