using server.Domain.Common;
using server.Domain.Errors;

namespace server.Domain.Entities;

public sealed class User : Entity
{
    private User()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException(DomainErrors.User.UsernameRequired, nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(DomainErrors.User.EmailRequired, nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(DomainErrors.User.PasswordHashRequired, nameof(passwordHash));

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException(DomainErrors.User.PasswordHashRequired, nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(DomainErrors.User.EmailRequired, nameof(email));

        Email = email;
    }

    public void ChangeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException(DomainErrors.User.UsernameRequired, nameof(username));

        Username = username;
    }
}
