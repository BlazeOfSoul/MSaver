using MSaver.Domain.Common;
using MSaver.Domain.Errors;

namespace MSaver.Domain.Entities;

public sealed class User : Entity
{
    private User()
    {
        Username = null!;
        Email = null!;
        PasswordHash = null!;
        CreatedAt = DateTime.UtcNow;
    }

    public string Username
    {
        get; private set;
    }
    public string Email
    {
        get; private set;
    }
    public string PasswordHash
    {
        get; private set;
    }
    public DateTime CreatedAt
    {
        get; private set;
    }

    public static User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException(UserDomainErrors.UsernameRequired);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(UserDomainErrors.EmailRequired);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(UserDomainErrors.PasswordHashRequired);

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException(UserDomainErrors.PasswordHashRequired);

        PasswordHash = newPasswordHash;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(UserDomainErrors.EmailRequired);

        Email = email;
    }

    public void ChangeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException(UserDomainErrors.UsernameRequired);

        Username = username;
    }
}
