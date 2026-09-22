using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Donations.Infrastructure.Persistence;

public sealed class DonationsUnitOfWork(DonationsDbContext context, ILogger<DonationsUnitOfWork> logger)
    : UnitOfWorkTemplate(context, logger)
{
    protected override bool TryClassifyAsConflict(DbUpdateException exception, out string reason)
    {
        if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            reason = "Donation already processed (duplicate detected at the database level).";
            return true;
        }

        reason = string.Empty;
        return false;
    }
}
