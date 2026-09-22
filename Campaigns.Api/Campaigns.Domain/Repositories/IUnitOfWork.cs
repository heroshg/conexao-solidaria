namespace Campaigns.Domain.Repositories;

/// <summary>
/// The only thing allowed to commit a transaction — repositories just stage changes
/// (Add/Update) via the tracked DbContext, they never call SaveChanges themselves.
/// </summary>
public interface IUnitOfWork
{
    Task<UnitOfWorkResult> CommitAsync(CancellationToken cancellationToken = default);
}

public sealed record UnitOfWorkResult(bool IsSuccess, string? Error)
{
    public static UnitOfWorkResult Success() => new(true, null);
    public static UnitOfWorkResult Failure(string error) => new(false, error);
}
