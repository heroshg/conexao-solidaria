using System.Text.RegularExpressions;
using Identity.Domain.Common;
using Identity.Domain.Exceptions;

namespace Identity.Domain.ValueObjects;

public sealed partial class Cpf : ValueObject
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Cpf Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Cpf is required.");

        var digits = DigitsOnlyPattern().Replace(value, string.Empty);

        if (digits.Length != 11 || !HasValidCheckDigits(digits))
            throw new DomainException($"'{value}' is not a valid CPF.");

        return new Cpf(digits);
    }

    // Standard Receita Federal CPF check-digit algorithm; also rejects all-same-digit
    // sequences (e.g. 111.111.111-11), which pass a naive length check but are never valid.
    private static bool HasValidCheckDigits(string digits)
    {
        if (digits.Distinct().Count() == 1) return false;

        var numbers = digits.Select(c => c - '0').ToArray();

        var firstCheckDigit = CalculateCheckDigit(numbers, 10);
        if (firstCheckDigit != numbers[9]) return false;

        var secondCheckDigit = CalculateCheckDigit(numbers, 11);
        return secondCheckDigit == numbers[10];
    }

    private static int CalculateCheckDigit(int[] numbers, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < weightStart - 1; i++)
            sum += numbers[i] * (weightStart - i);

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex DigitsOnlyPattern();
}
