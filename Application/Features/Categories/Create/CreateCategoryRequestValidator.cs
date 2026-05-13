namespace MSaver.Application.Features.Categories.Create;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(50).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(20).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}