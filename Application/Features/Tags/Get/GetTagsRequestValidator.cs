using MSaver.Api.Contracts.Tags;

namespace MSaver.Application.Features.Tags.Get;

public sealed class GetTagsRequestValidator : AbstractValidator<GetTagsRequest>
{
    public GetTagsRequestValidator()
    {
        RuleFor(x => x.Page)
            .ValidPage();

        RuleFor(x => x.Size)
            .ValidPageSize(ListQueryDefaults.MaxPageSize);

        RuleFor(x => x.SortBy)
            .ValidSortBy(TagSortFields.All);

        RuleFor(x => x.SortDirection)
            .ValidSortDirection();

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.MaxLength);
    }
}