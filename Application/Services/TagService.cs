using MSaver.Application.Features.Tags.AssignCategories;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Get;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.Application.Services;

public sealed class TagService(
    IUserRepository userRepository,
    ITagRepository tagRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ITagService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<CreateTagResponse>> CreateAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<CreateTagResponse>.Failure(UserDomainErrors.UserNotFound);

        var exists = await _tagRepository.ExistsByNameAsync(
            userId,
            request.Name,
            cancellationToken);

        if (exists)
            return Result<CreateTagResponse>.Failure(TagDomainErrors.NameAlreadyExists);

        try
        {
            var tag = Domain.Entities.Tag.Create(
                userId,
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
        var userId = _currentUserService.UserId;

        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tag is null)
            return Result<Guid>.Failure(TagDomainErrors.TagNotFound);

        if (tag.UserId != userId)
            return Result<Guid>.Failure(TagDomainErrors.AccessDenied);

        if (tag.IsDeleted)
            return Result<Guid>.Failure(TagDomainErrors.TagDeleted);

        var exists = await _tagRepository.ExistsByNameAsync(
            userId,
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
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);

        if (tag is null)
            return Result<Guid>.Failure(TagDomainErrors.TagNotFound);

        if (tag.UserId != userId)
            return Result<Guid>.Failure(TagDomainErrors.AccessDenied);

        if (tag.IsDeleted)
            return Result<Guid>.Failure(TagDomainErrors.TagAlreadyDeleted);

        tag.Delete();

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }

    public async Task<Result<GetTagsResponse>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var tags = await _tagRepository.GetAsync(userId, cancellationToken);

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

    public async Task<Result<Guid>> AssignCategoriesAsync(
        AssignTagCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var tag = await _tagRepository.GetByIdWithCategoriesAsync(request.TagId, cancellationToken);

        if (tag is null)
            return Result<Guid>.Failure(TagDomainErrors.TagNotFound);

        if (tag.UserId != userId)
            return Result<Guid>.Failure(TagDomainErrors.AccessDenied);

        if (tag.IsDeleted)
            return Result<Guid>.Failure(TagDomainErrors.TagDeleted);

        var categoryIds = request.CategoryIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var categories = await _categoryRepository.GetByIdsAsync(
            userId,
            categoryIds,
            cancellationToken);

        if (categories.Count != categoryIds.Length)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryNotFound);

        try
        {
            tag.ReplaceCategories(categoryIds);

            await _tagRepository.UpdateAsync(tag, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(tag.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }
}