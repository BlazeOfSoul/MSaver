using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TagsController(
    ITagService tagService,
    IValidator<CreateTagRequest> createValidator,
    IValidator<UpdateTagRequest> updateValidator) : ApiControllerBase
{
    private readonly ITagService _tagService = tagService;
    private readonly IValidator<CreateTagRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateTagRequest> _updateValidator = updateValidator;

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
        return ValidateAndExecuteAsync<CreateTagRequest, CreateTagResponse>(
            request,
            _createValidator,
            ct => _tagService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTagRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTagRequest
        {
            Id = id,
            Name = request.Name,
            Color = request.Color
        };

        return ValidateAndExecuteAsync<UpdateTagRequest, Guid>(
            command,
            _updateValidator,
            ct => _tagService.UpdateAsync(command, ct),
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
}