using Donations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donations.Infrastructure.Persistence.Configurations;

public class ProcessedDonationConfiguration : IEntityTypeConfiguration<ProcessedDonation>
{
    public void Configure(EntityTypeBuilder<ProcessedDonation> builder)
    {
        // Own schema: makes ownership obvious even though it's the same physical DB as Campaigns.Api.
        builder.ToTable("ProcessedDonations", "donations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CampaignId).IsRequired();
        builder.Property(x => x.DonorId).IsRequired();
        builder.Property(x => x.DonationAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();
    }
}
