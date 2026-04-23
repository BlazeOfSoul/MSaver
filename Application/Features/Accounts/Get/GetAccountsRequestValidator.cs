using MSaver.Api.Contracts.Accounts;
using MSaver.Application.Common.Models;
using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Accounts.Get;

public sealed class GetAccountsRequestValidator : AbstractValidator<GetAccountsRequest>
{
    public GetAccountsRequestValidator()
    {
        RuleFor(x => x.Page)
            .ValidPage();

        RuleFor(x => x.Size)
            .ValidPageSize(ListQueryDefaults.MaxPageSize);

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(10)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.SortBy)
            .ValidSortBy(AccountSortFields.All);

        RuleFor(x => x.SortDirection)
            .ValidSortDirection();
    }
}