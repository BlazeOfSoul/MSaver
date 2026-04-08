namespace MSaver.Domain.Entities;

public sealed class User : Entity
{
    private readonly List<Account> _accounts = [];
    private readonly List<Category> _categories = [];
    private readonly List<Tag> _tags = [];
    private readonly List<Transaction> _transactions = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User() { }

    public string Name { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Account> Accounts => _accounts;

    public IReadOnlyCollection<Category> Categories => _categories;

    public IReadOnlyCollection<Tag> Tags => _tags;

    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public static User Create(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(UserDomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(UserDomainErrors.EmailRequired);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(UserDomainErrors.PasswordHashRequired);

        return new User
        {
            Name = name.Trim(),
            Email = email.Trim(),
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

        Email = email.Trim();
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(UserDomainErrors.NameRequired);

        Name = name.Trim();
    }
}