using MSaver.Application.Features.Users.Get;

namespace MSaver.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    ICurrentUserService currentUserService) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<CurrentUserResponse>> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<CurrentUserResponse>.Failure(UserDomainErrors.UserNotFound);

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.Name,
            user.Email));
    }
}