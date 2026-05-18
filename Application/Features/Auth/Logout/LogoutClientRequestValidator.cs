namespace MSaver.Application.Features.Auth.Logout;

public sealed class LogoutClientRequestValidator : AbstractValidator<LogoutClientRequest>
{
    public LogoutClientRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .MaximumLength(64);
    }
}