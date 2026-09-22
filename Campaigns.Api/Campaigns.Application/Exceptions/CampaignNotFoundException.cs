namespace Campaigns.Application.Exceptions;

public sealed class CampaignNotFoundException(Guid id) : Exception($"Campaign '{id}' was not found.");
