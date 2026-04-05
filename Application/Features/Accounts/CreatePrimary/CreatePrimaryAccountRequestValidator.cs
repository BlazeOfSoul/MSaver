 namespace MSaver.Application.Features.Accounts.CreatePrimary;

public sealed class CreatePrimaryAccountRequestValidator : AbstractValidator<CreatePrimaryAccountRequest>
{
    public CreatePrimaryAccountRequestValidator()
    {
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}