using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Delete;
using MSaver.Application.Features.Tags.Get;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TagsController(
    ITagService tagService,
    ICurrentUserService currentUser,
    IValidator<CreateTagRequest> createValidator,
    IValidator<UpdateTagRequest> updateValidator) : ApiControllerBase
{
    private readonly ITagService _tagService = tagService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IValidator<CreateTagRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateTagRequest> _updateValidator = updateValidator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var request = new GetTagsRequest(_currentUser.UserId);
        var result = await _tagService.GetAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = _currentUser.UserId;

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
        request.Id = id;
        request.UserId = _currentUser.UserId;

        return ValidateAndExecuteAsync<UpdateTagRequest, Guid>(
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
        var request = new DeleteTagRequest(id, _currentUser.UserId);
        var result = await _tagService.DeleteAsync(request, cancellationToken);
        return FromResult(result);
    }
}