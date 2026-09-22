using Donations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donations.Infrastructure.Persistence.Configurations;

public class CampaignAmountRecordConfiguration : IEntityTypeConfiguration<CampaignAmountRecord>
{
    public void Configure(EntityTypeBuilder<CampaignAmountRecord> builder)
    {
        // Same physical table Campaigns.Api owns and migrates — this context only ever
        // reads/writes Status and AmountRaised, and NEVER generates migrations for it.
        builder.ToTable("Campaigns", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.AmountRaised).HasColumnName("AmountRaised").HasColumnType("numeric(18,2)");
    }
}
