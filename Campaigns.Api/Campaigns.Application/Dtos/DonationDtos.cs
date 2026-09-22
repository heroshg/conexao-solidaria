namespace Campaigns.Application.Dtos;

public sealed record CreateDonationRequest(Guid CampaignId, decimal DonationAmount);

public sealed record CreateDonationResponse(Guid DonationId, Guid CampaignId, Guid DonorId, decimal DonationAmount);
