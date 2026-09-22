namespace Campaigns.Application.Exceptions;

public sealed class CampaignNotReceivingDonationsException(Guid id)
    : Exception($"Campaign '{id}' is not accepting donations (status is Completed or Cancelled).");
