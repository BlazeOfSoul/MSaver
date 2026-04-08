using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Accounts.GetMonthBalance;

public sealed class GetMonthBalanceRequestValidator : AbstractValidator<GetMonthBalanceRequest>
{
    public GetMonthBalanceRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage(ValidationMessages.InvalidDate);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage(ValidationMessages.InvalidDate);
    }
}