namespace Donations.Domain.Repositories;

/// <summary>
/// Commits ProcessedDonation insert + AmountRaised increment together in one DB transaction
/// (both go through the same DbContext) — see CLAUDE.md idempotency rule. A unique-constraint
/// violation on ProcessedDonation.Id (= DonationId) means a concurrent redelivery already won
/// the race; that's the actual idempotency guard, not just the upfront ExistsAsync check.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// <paramref name="beforeSaveChanges"/> runs inside the same open transaction, before
    /// SaveChangesAsync — for atomic SQL operations (e.g. an ExecuteUpdate-based increment)
    /// that must commit-or-rollback together with the tracked-entity changes, without EF's
    /// change tracker ever materializing/overwriting the column with a stale in-memory value.
    /// </summary>
    Task<UnitOfWorkResult> CommitAsync(
        Func<CancellationToken, Task>? beforeSaveChanges = null, CancellationToken cancellationToken = default);
}

public sealed record UnitOfWorkResult(bool IsSuccess, string? Error)
{
    public static UnitOfWorkResult Success() => new(true, null);
    public static UnitOfWorkResult Failure(string error) => new(false, error);
}
