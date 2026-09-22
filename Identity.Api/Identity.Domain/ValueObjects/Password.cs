using Identity.Domain.Common;
using Identity.Domain.Exceptions;

namespace Identity.Domain.ValueObjects;

/// <summary>Raw (plaintext) password, validated before hashing. Never persisted.</summary>
public sealed class Password : ValueObject
{
    private const int MinLength = 8;

    // Argon2id hashes the raw input with no truncation — an unbounded password string is a
    // CPU-amplification vector (a few KB of body on POST /donors costs the caller almost
    // nothing but forces a full Argon2id pass over all of it). 128 is generous for any real
    // passphrase while closing that off.
    private const int MaxLength = 128;

    public string Value { get; }

    private Password(string value) => Value = value;

    public static Password Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < MinLength)
            throw new DomainException($"Password must be at least {MinLength} characters long.");

        if (value.Length > MaxLength)
            throw new DomainException($"Password cannot exceed {MaxLength} characters.");

        return new Password(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
