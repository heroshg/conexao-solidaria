using Donations.Domain.Entities;
using Donations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Donations.Infrastructure.Persistence;

public class CampaignAmountRepository(DonationsDbContext context) : ICampaignAmountRepository
{
    public Task<CampaignAmountRecord?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default) =>
        context.CampaignAmounts.SingleOrDefaultAsync(x => x.Id == campaignId, cancellationToken);

    public async Task<bool> IncreaseAmountRaisedAsync(
        Guid campaignId, decimal amount, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await context.CampaignAmounts
            .Where(x => x.Id == campaignId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.AmountRaised, x => x.AmountRaised + amount),
                cancellationToken);

        return rowsAffected == 1;
    }
}
