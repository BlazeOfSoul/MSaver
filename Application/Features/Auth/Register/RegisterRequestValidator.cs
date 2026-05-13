namespace MSaver.Application.Features.Auth.Register;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(50).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(6).WithMessage(ValidationMessages.MinLength)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength)
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Пароль должен содержать хотя бы один специальный символ.");
    }
}