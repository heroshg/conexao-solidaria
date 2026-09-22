using Identity.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Entities;

public abstract class UserAccount : Entity
{
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Role Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // EF Core materialization constructor.
    protected UserAccount() { }

    protected UserAccount(Email email, string passwordHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
