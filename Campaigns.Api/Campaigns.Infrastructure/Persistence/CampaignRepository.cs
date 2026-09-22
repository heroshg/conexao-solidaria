using Campaigns.Domain.Entities;
using Campaigns.Domain.Enums;
using Campaigns.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Campaigns.Infrastructure.Persistence;

public class CampaignRepository(CampaignsDbContext context) : ICampaignRepository
{
    public Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Campaigns.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default) =>
        await context.Campaigns
            .Where(x => x.Status == CampaignStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default) =>
        await context.Campaigns.AddAsync(campaign, cancellationToken);
}
