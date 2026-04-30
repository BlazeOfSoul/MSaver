using MSaver.Api.Contracts.Tags;
using MSaver.Application.Features.Tags.AssignCategories;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Get;
using MSaver.Application.Features.Tags.GetById;
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

    public async Task<Result<GetTagsResponse>> GetAsync(
        GetTagsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var query = new TagListQuery
        {
            UserId = userId,
            Search = ListQueryHelper.NormalizeSearch(request.Search),
            SortBy = ListQueryHelper.NormalizeSortBy(request.SortBy, TagSortFields.Name),
            SortDirection = ListQueryHelper.NormalizeSortDirection(request.SortDirection),
            Page = request.Page,
            Size = request.Size
        };

        var pagedTags = await _tagRepository.GetPagedAsync(query, cancellationToken);

        var items = pagedTags.Items
            .Select(x => new TagItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color
            })
            .ToArray();

        return Result<GetTagsResponse>.Success(new GetTagsResponse
        {
            Items = items,
            Page = pagedTags.Page,
            Size = pagedTags.Size,
            TotalCount = pagedTags.TotalCount,
            TotalPages = pagedTags.TotalPages,
            HasPreviousPage = pagedTags.HasPreviousPage,
            HasNextPage = pagedTags.HasNextPage
        });
    }

    public async Task<Result<GetTagByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var tag = await _tagRepository.GetByIdWithCategoriesAsync(id, cancellationToken);

        if (tag is null)
            return Result<GetTagByIdResponse>.Failure(TagDomainErrors.TagNotFound);

        if (tag.UserId != userId)
            return Result<GetTagByIdResponse>.Failure(TagDomainErrors.AccessDenied);

        var response = new GetTagByIdResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            IsDeleted = tag.IsDeleted,
            Categories = tag.TagCategories
                .Where(x => x.Category is not null)
                .OrderBy(x => x.Category!.Name)
                .Select(x => new TagCategoryResponse
                {
                    Id = x.CategoryId,
                    Name = x.Category!.Name,
                    Color = x.Category.Color,
                    Type = x.Category.Type.ToString(),
                    IsDeleted = x.Category.IsDeleted
                })
                .ToArray()
        };

        return Result<GetTagByIdResponse>.Success(response);
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<Guid>.Failure(UserDomainErrors.UserNotFound);

        var exists = await _tagRepository.ExistsByNameAsync(
            userId,
            request.Name,
            cancellationToken);

        if (exists)
            return Result<Guid>.Failure(TagDomainErrors.NameAlreadyExists);

        var tag = Tag.Create(
            userId,
            request.Name,
            request.Color);

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
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

        tag.Update(request.Name, request.Color);

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
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
            return Result<Guid>.Failure(CategoryDomainErrors.NotFound);

        tag.ReplaceCategories(categoryIds);

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }
}