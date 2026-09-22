using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Entities;

public sealed class Donor : UserAccount
{
    public string FullName { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;

    private Donor() { }

    private Donor(string fullName, Email email, Cpf cpf, string passwordHash)
        : base(email, passwordHash, Role.Donor)
    {
        FullName = fullName;
        Cpf = cpf;
    }

    public static Donor Register(string fullName, Email email, Cpf cpf, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        return new Donor(fullName.Trim(), email, cpf, passwordHash);
    }
}
