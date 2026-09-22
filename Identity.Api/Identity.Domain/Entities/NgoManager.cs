using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Entities;

/// <summary>
/// NgoManager accounts are not self-registrable (no public endpoint) — created only via
/// the Identity.Api startup seed. See CLAUDE.md "Regras de negócio".
/// </summary>
public sealed class NgoManager : UserAccount
{
    public string FullName { get; private set; } = null!;

    private NgoManager() { }

    private NgoManager(string fullName, Email email, string passwordHash)
        : base(email, passwordHash, Role.NgoManager)
    {
        FullName = fullName;
    }

    public static NgoManager CreateSeedAdmin(string fullName, Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        return new NgoManager(fullName.Trim(), email, passwordHash);
    }
}
