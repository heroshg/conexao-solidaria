using Campaigns.Application.Dtos;
using Campaigns.Application.Mapping;
using Campaigns.Domain.Repositories;

namespace Campaigns.Application.UseCases;

public sealed class GetPublicCampaignsUseCase(ICampaignRepository repository)
{
    public async Task<IReadOnlyList<PublicCampaignResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var campaigns = await repository.GetActiveCampaignsAsync(cancellationToken);
        return campaigns.Select(CampaignMapper.ToPublicResponse).ToList();
    }
}
