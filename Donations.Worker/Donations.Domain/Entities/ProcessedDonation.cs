using Donations.Domain.Common;

namespace Donations.Domain.Entities;

/// <summary>
/// Idempotency ledger: one row per successfully applied DonationId. Its existence is what
/// stops a redelivered DonationReceivedEvent from being counted twice — see CLAUDE.md.
/// </summary>
public sealed class ProcessedDonation : Entity
{
    public Guid CampaignId { get; private set; }
    public Guid DonorId { get; private set; }
    public decimal DonationAmount { get; private set; }
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedDonation() { }

    private ProcessedDonation(Guid donationId, Guid campaignId, Guid donorId, decimal donationAmount)
        : base(donationId)
    {
        CampaignId = campaignId;
        DonorId = donorId;
        DonationAmount = donationAmount;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public static ProcessedDonation Create(Guid donationId, Guid campaignId, Guid donorId, decimal donationAmount) =>
        new(donationId, campaignId, donorId, donationAmount);
}
