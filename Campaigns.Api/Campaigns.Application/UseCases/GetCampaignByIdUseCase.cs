using Campaigns.Application.Dtos;
using Campaigns.Application.Exceptions;
using Campaigns.Application.Mapping;
using Campaigns.Domain.Repositories;

namespace Campaigns.Application.UseCases;

/// <summary>
/// Lets a Donor resolve a Campaign's Id before calling POST /donations — the public listing
/// intentionally omits Id per its documented contract (Title/FinancialGoal/AmountRaised only),
/// so this is the only way to discover it without an NgoManager session.
/// </summary>
public sealed class GetCampaignByIdUseCase(ICampaignRepository repository)
{
    public async Task<CampaignResponse> ExecuteAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await repository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new CampaignNotFoundException(campaignId);

        return CampaignMapper.ToResponse(campaign);
    }
}
