using MSaver.Application.Features.Users.Get;

using MSaver.Application.Features.Users.UpdateApplicationCurrency;

namespace MSaver.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

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
            user.Email,
            user.ApplicationCurrencyCode));
    }

    public async Task<Result<CurrentUserResponse>> UpdateApplicationCurrencyAsync(
        UpdateApplicationCurrencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<CurrentUserResponse>.Failure(UserDomainErrors.UserNotFound);

        try
        {
            user.ChangeApplicationCurrency(request.ApplicationCurrencyCode);
        }
        catch (DomainException exception)
        {
            return Result<CurrentUserResponse>.Failure(exception.Error);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.ApplicationCurrencyCode));
    }
}
