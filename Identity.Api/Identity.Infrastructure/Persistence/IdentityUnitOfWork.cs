using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityUnitOfWork(IdentityDbContext context, ILogger<IdentityUnitOfWork> logger)
    : UnitOfWorkTemplate(context, logger)
{
    protected override bool TryClassifyAsConflict(DbUpdateException exception, out string reason)
    {
        if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            reason = "Email or CPF already registered.";
            return true;
        }

        reason = string.Empty;
        return false;
    }
}
