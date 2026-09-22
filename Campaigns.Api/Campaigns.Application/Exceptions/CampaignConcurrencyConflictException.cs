namespace Campaigns.Application.Exceptions;

public sealed class CampaignConcurrencyConflictException(Guid id)
    : Exception($"Campaign '{id}' was modified concurrently — reload and try again.");
