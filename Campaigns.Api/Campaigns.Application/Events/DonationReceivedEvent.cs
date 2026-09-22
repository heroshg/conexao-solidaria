namespace Campaigns.Application.Events;

/// <summary>
/// Published by Campaigns.Api, consumed by Donations.Worker. Intentionally duplicated in
/// Donations.Application (same shape) instead of a shared contracts project — see CLAUDE.md
/// "Estrutura de pastas sugerida". Keep both copies' fields identical by hand.
/// </summary>
public sealed record DonationReceivedEvent(Guid DonationId, Guid CampaignId, Guid DonorId, decimal DonationAmount);
