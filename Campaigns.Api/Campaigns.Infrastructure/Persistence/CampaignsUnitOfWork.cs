using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Campaigns.Infrastructure.Persistence;

public sealed class CampaignsUnitOfWork(CampaignsDbContext context, ILogger<CampaignsUnitOfWork> logger)
    : UnitOfWorkTemplate(context, logger)
{
    // Campaign uses the Postgres xmin system column as an optimistic concurrency token (see
    // CampaignConfiguration) — two concurrent PUT /campaigns/{id} requests now surface a clean
    // 409 here instead of the second one silently clobbering the first's write.
    protected override bool TryClassifyAsConflict(DbUpdateException exception, out string reason)
    {
        if (exception is DbUpdateConcurrencyException)
        {
            reason = "Campaign was modified concurrently.";
            return true;
        }

        reason = string.Empty;
        return false;
    }
}
