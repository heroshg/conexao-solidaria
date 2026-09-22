using Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Template Method: fixes the commit algorithm (begin transaction -> SaveChanges -> commit,
/// or rollback on failure) so no repository or use case ever calls SaveChanges/try-catch
/// itself. The only thing a concrete subclass decides is <see cref="TryClassifyAsConflict"/>:
/// whether a given DbUpdateException is an EXPECTED, swallowable conflict (returned as a
/// Result the caller checks) or a genuine failure (rethrown after rollback, so it reaches the
/// normal failure path — the Global Exception Handler here, or MassTransit's retry/DLQ for a
/// message consumer — instead of being silently absorbed).
/// </summary>
public abstract class UnitOfWorkTemplate(DbContext context, ILogger logger) : IUnitOfWork
{
    public async Task<UnitOfWorkResult> CommitAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return UnitOfWorkResult.Success();
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            if (TryClassifyAsConflict(exception, out var reason))
            {
                logger.LogWarning(exception, "Unit of work commit conflict: {Reason}", reason);
                return UnitOfWorkResult.Failure(reason);
            }

            logger.LogError(exception, "Unit of work commit failed unexpectedly.");
            throw;
        }
    }

    protected virtual bool TryClassifyAsConflict(DbUpdateException exception, out string reason)
    {
        reason = string.Empty;
        return false;
    }
}
