using Campaigns.Application.Events;
using Donations.Domain.Entities;
using Donations.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Donations.Application.UseCases;

public sealed class ApplyDonationUseCase(
    IProcessedDonationRepository processedDonationRepository,
    ICampaignAmountRepository campaignAmountRepository,
    IUnitOfWork unitOfWork,
    ILogger<ApplyDonationUseCase> logger)
{
    public async Task ExecuteAsync(DonationReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        if (await processedDonationRepository.ExistsAsync(@event.DonationId, cancellationToken))
        {
            logger.LogInformation("Donation {DonationId} already processed — skipping (redelivery).", @event.DonationId);
            return;
        }

        if (@event.DonationAmount <= 0)
        {
            // Defense in depth: Campaigns.Api already validated this before publishing. A
            // zero/negative amount reaching the queue is a poisoned message, not a retryable
            // fault — discard it the same way an inactive/missing campaign is discarded below,
            // instead of applying it (which would silently decrease AmountRaised for a
            // negative value, or no-op for zero while still marking it "processed").
            logger.LogWarning(
                "Donation {DonationId} has a non-positive amount {DonationAmount} — discarding.",
                @event.DonationId, @event.DonationAmount);
            return;
        }

        var campaign = await campaignAmountRepository.GetByIdAsync(@event.CampaignId, cancellationToken);
        if (campaign is null || !campaign.IsActive)
        {
            // Defense in depth: Campaigns.Api already validated this before publishing.
            logger.LogWarning(
                "Campaign {CampaignId} not found or not Active — discarding donation {DonationId}.",
                @event.CampaignId, @event.DonationId);
            return;
        }

        var processedDonation = ProcessedDonation.Create(@event.DonationId, @event.CampaignId, @event.DonorId, @event.DonationAmount);
        await processedDonationRepository.AddAsync(processedDonation, cancellationToken);

        // The atomic increment runs inside the same transaction as the ProcessedDonations
        // insert above (see IUnitOfWork.CommitAsync) — if the insert then fails on redelivery
        // (duplicate DonationId), the whole transaction rolls back and the increment never
        // takes effect either.
        var result = await unitOfWork.CommitAsync(
            beforeSaveChanges: ct => campaignAmountRepository.IncreaseAmountRaisedAsync(@event.CampaignId, @event.DonationAmount, ct),
            cancellationToken: cancellationToken);
        if (!result.IsSuccess)
        {
            // Most likely cause: a concurrent redelivery of the same message already committed
            // first — exactly the idempotency guarantee this transaction exists to enforce.
            // Not an error: the donation WAS applied, just by the other delivery.
            logger.LogInformation(
                "Donation {DonationId} commit skipped — {Reason}", @event.DonationId, result.Error);
        }
    }
}
