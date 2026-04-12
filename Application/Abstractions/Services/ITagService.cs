using MSaver.Application.Features.Tags.AssignCategories;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Get;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Application.Abstractions.Services;

public interface ITagService
{
    Task<Result<Guid>> CreateAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateTagRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<GetTagsResponse>> GetAsync(
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> AssignCategoriesAsync(
        AssignTagCategoriesRequest request,
        CancellationToken cancellationToken = default);
}