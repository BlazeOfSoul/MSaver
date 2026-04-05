using FluentValidation;

using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Tags.AssignCategories;

public sealed class AssignTagCategoriesRequestValidator : AbstractValidator<AssignTagCategoriesRequest>
{
    public AssignTagCategoriesRequestValidator()
    {
        RuleFor(x => x.TagId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.CategoryIds)
            .NotNull()
            .WithMessage(ValidationMessages.CollectionRequired);

        RuleFor(x => x.CategoryIds)
            .Must(x => x is not null)
            .WithMessage(ValidationMessages.CollectionRequired);

        RuleFor(x => x.CategoryIds)
            .Must(x => x is not null && x.All(id => id != Guid.Empty))
            .WithMessage(ValidationMessages.InvalidId);
    }
}