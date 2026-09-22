using Campaigns.Domain.Common;
using Campaigns.Domain.Exceptions;

namespace Campaigns.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    // Stays comfortably under the numeric(18,2) column's 16-digit integer-part ceiling so a
    // legitimate large value can never trigger a Postgres numeric-field-overflow error at
    // SaveChangesAsync — which UnitOfWorkTemplate doesn't classify as a conflict, so it would
    // otherwise surface as an unhandled 500 (API) or exhaust MassTransit retries into the DLQ
    // (Worker) instead of a clean 400 at the API boundary.
    public const decimal MaxAmount = 999_999_999_999_999.99m;

    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");

        if (amount > MaxAmount)
            throw new DomainException($"Amount cannot exceed {MaxAmount:F2}.");

        return new Money(amount);
    }

    public static Money Zero() => new(0);

    public Money Add(Money other) => new(Amount + other.Amount);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("F2");
}
