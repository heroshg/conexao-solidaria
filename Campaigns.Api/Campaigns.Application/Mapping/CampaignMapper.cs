using Campaigns.Application.Dtos;
using Campaigns.Domain.Entities;

namespace Campaigns.Application.Mapping;

public static class CampaignMapper
{
    public static CampaignResponse ToResponse(Campaign campaign) => new(
        campaign.Id, campaign.Title, campaign.Description, campaign.StartDate, campaign.EndDate,
        campaign.FinancialGoal.Amount, campaign.AmountRaised.Amount, campaign.Status.ToString());

    public static PublicCampaignResponse ToPublicResponse(Campaign campaign) => new(
        campaign.Title, campaign.FinancialGoal.Amount, campaign.AmountRaised.Amount);
}
