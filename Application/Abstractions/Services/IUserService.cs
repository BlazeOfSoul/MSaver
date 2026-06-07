using MSaver.Application.Features.Users.Get;

using MSaver.Application.Features.Users.UpdateApplicationCurrency;

namespace MSaver.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result<CurrentUserResponse>> GetCurrentAsync(
        CancellationToken cancellationToken = default);

    Task<Result<CurrentUserResponse>> UpdateApplicationCurrencyAsync(
        UpdateApplicationCurrencyRequest request,
        CancellationToken cancellationToken = default);
}
