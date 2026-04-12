using MSaver.Application.Features.Users.Get;

namespace MSaver.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result<CurrentUserResponse>> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}