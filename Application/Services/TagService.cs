using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Delete;
using MSaver.Application.Features.Tags.Get;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Application.Services;

public sealed class TagService(
    IUserRepository userRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : ITagService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CreateTagResponse>> CreateAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<CreateTagResponse>.Failure(UserDomainErrors.UserNotFound);

        var exists = await _tagRepository.ExistsByNameAsync(
            request.UserId,
            request.Name,
            cancellationToken);

        if (exists)
            return Result<CreateTagResponse>.Failure(TagDomainErrors.NameAlreadyExists);

        try
        {
            var tag = Domain.Entities.Tag.Create(
                request.UserId,
                request.Name,
                request.Color);

            await _tagRepository.AddAsync(tag, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateTagResponse>.Success(new CreateTagResponse(tag.Id));
        }
        catch (DomainException ex)
        {
            return Result<CreateTagResponse>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tag is null)
            return Result<Guid>.Failure(TagDomainErrors.TagNotFound);

        if (tag.UserId != request.UserId)
            return Result<Guid>.Failure(TagDomainErrors.AccessDenied);

        if (tag.IsDeleted)
            return Result<Guid>.Failure(TagDomainErrors.TagDeleted);

        var exists = await _tagRepository.ExistsByNameAsync(
            request.UserId,
            request.Name,
            cancellationToken,
            request.Id);

        if (exists)
            return Result<Guid>.Failure(TagDomainErrors.NameAlreadyExists);

        try
        {
            tag.Update(request.Name, request.Color);

            await _tagRepository.UpdateAsync(tag, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(tag.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> DeleteAsync(
        DeleteTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tag is null)
            return Result<Guid>.Failure(TagDomainErrors.TagNotFound);

        if (tag.UserId != request.UserId)
            return Result<Guid>.Failure(TagDomainErrors.AccessDenied);

        if (tag.IsDeleted)
            return Result<Guid>.Failure(TagDomainErrors.TagAlreadyDeleted);

        tag.Delete();

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }

    public async Task<Result<GetTagsResponse>> GetAsync(
        GetTagsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAsync(request.UserId, cancellationToken);

        var items = tags
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new TagItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color
            })
            .ToArray();

        return Result<GetTagsResponse>.Success(new GetTagsResponse
        {
            Items = items
        });
    }
}