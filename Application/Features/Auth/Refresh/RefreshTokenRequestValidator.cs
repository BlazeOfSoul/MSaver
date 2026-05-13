namespace MSaver.Application.Features.Auth.Refresh;

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(500).WithMessage(ValidationMessages.MaxLength);
    }
}