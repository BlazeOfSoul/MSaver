using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Auth.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(6).WithMessage(ValidationMessages.MinLength)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength);
    }
}