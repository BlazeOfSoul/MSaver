using MSaver.Application.Common.Validation;
using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Update;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(50).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(20).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Must(x => Enum.TryParse<CategoryType>(x, true, out _))
            .WithMessage("Некорректный тип категории.");
    }
}