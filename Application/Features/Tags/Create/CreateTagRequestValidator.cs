namespace MSaver.Application.Features.Tags.Create;

public sealed class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Color)
            .MaximumLength(20)
            .WithMessage(ValidationMessages.MaxLength);
    }
}