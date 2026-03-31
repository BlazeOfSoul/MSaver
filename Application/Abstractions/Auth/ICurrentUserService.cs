namespace MSaver.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    Guid UserId
    {
        get;
    }
    string? Username
    {
        get;
    }
    string? Email
    {
        get;
    }
    bool IsAuthenticated
    {
        get;
    }
}