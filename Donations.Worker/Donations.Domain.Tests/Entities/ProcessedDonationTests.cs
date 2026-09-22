using Donations.Domain.Entities;

namespace Donations.Domain.Tests.Entities;

public class ProcessedDonationTests
{
    [Fact]
    public void Create_SetsAllFieldsFromArguments()
    {
        var donationId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var donorId = Guid.NewGuid();

        var processedDonation = ProcessedDonation.Create(donationId, campaignId, donorId, 150m);

        Assert.Equal(donationId, processedDonation.Id);
        Assert.Equal(campaignId, processedDonation.CampaignId);
        Assert.Equal(donorId, processedDonation.DonorId);
        Assert.Equal(150m, processedDonation.DonationAmount);
        Assert.True(processedDonation.ProcessedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_UsesDonationIdAsEntityId_ForIdempotencyLookup()
    {
        var donationId = Guid.NewGuid();

        var processedDonation = ProcessedDonation.Create(donationId, Guid.NewGuid(), Guid.NewGuid(), 10m);

        // The idempotency check in ApplyDonationUseCase looks up by this Id — it must be the
        // DonationId, not a freshly generated one, or redelivery would never be detected.
        Assert.Equal(donationId, processedDonation.Id);
    }
}
