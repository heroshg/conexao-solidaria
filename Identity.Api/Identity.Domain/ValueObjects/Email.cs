using System.Text.RegularExpressions;
using Identity.Domain.Common;
using Identity.Domain.Exceptions;

namespace Identity.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailPattern().IsMatch(normalized))
            throw new DomainException($"'{value}' is not a valid email address.");

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Domain is modeled as label(.label)*.TLD with each label alnum/hyphen (no leading/
    // trailing hyphen, no empty label) — rejects "user@a..com" (empty label between dots) and
    // "user@example.com." (trailing dot) that the previous "anything but @/whitespace" pattern
    // let through on both sides of the final dot.
    [GeneratedRegex(@"^[^@\s]+@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}$")]
    private static partial Regex EmailPattern();
}
