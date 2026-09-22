using Campaigns.Domain.Entities;
using Campaigns.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campaigns.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();

        builder.Property(x => x.FinancialGoal)
            .HasConversion(money => money.Amount, value => Money.Create(value))
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        // Only Donations.Worker's own DbContext writes to this column — see CLAUDE.md.
        builder.Property(x => x.AmountRaised)
            .HasConversion(money => money.Amount, value => Money.Create(value))
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Optimistic concurrency via Postgres's built-in xmin system column — no extra column/
        // migration needed. Guards concurrent PUT /campaigns/{id} lost-update races.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
