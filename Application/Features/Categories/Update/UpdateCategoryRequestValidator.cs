using MSaver.Application.Features.Categories.Common;
using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Update;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .MaximumLength(20)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Type)
            .IsInEnum()
            .Must(x => x is not CategoryType.TransferIncome and not CategoryType.TransferExpense)
            .WithMessage(CategoryValidationMessages.TransferCategoryTypeIsSystemOnly);
    }
}
