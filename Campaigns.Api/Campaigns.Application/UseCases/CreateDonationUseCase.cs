using Campaigns.Application.Abstractions;
using Campaigns.Application.Dtos;
using Campaigns.Application.Events;
using Campaigns.Application.Exceptions;
using Campaigns.Domain.Exceptions;
using Campaigns.Domain.Repositories;

namespace Campaigns.Application.UseCases;

/// <summary>
/// Validates and publishes the donation intent — fail fast here (existence + status), per
/// CLAUDE.md. Never writes AmountRaised; only Donations.Worker does, after consuming the event.
/// </summary>
public sealed class CreateDonationUseCase(ICampaignRepository repository, IEventPublisher eventPublisher)
{
    public async Task<CreateDonationResponse> ExecuteAsync(
        CreateDonationRequest request, Guid donorId, CancellationToken cancellationToken = default)
    {
        if (request.DonationAmount <= 0)
            throw new DomainException("DonationAmount must be greater than zero.");

        var campaign = await repository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new CampaignNotFoundException(request.CampaignId);

        if (!campaign.CanReceiveDonations())
            throw new CampaignNotReceivingDonationsException(request.CampaignId);

        var donationId = Guid.NewGuid();

        await eventPublisher.PublishAsync(
            new DonationReceivedEvent(donationId, campaign.Id, donorId, request.DonationAmount),
            cancellationToken);

        return new CreateDonationResponse(donationId, campaign.Id, donorId, request.DonationAmount);
    }
}
