using server.Domain.Common;
using server.Domain.Errors;

namespace server.Domain.Entities;

public sealed class User : Entity
{
    private User()
    {
        Username = null!;
        Email = null!;
        PasswordHash = null!;
        CreatedAt = DateTime.UtcNow;
    }

    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static User Create(
        string username,
        string email,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException(UserErrors.UsernameRequired, nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(UserErrors.EmailRequired, nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(UserErrors.PasswordHashRequired, nameof(passwordHash));

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
            throw new ArgumentException(UserErrors.PasswordHashRequired, nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(UserErrors.EmailRequired, nameof(email));

        Email = email;
    }

    public void ChangeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException(UserErrors.UsernameRequired, nameof(username));

        Username = username;
    }
}
