using Donations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Donations.Infrastructure.Persistence;

public class DonationsDbContext(DbContextOptions<DonationsDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedDonation> ProcessedDonations => Set<ProcessedDonation>();
    public DbSet<CampaignAmountRecord> CampaignAmounts => Set<CampaignAmountRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DonationsDbContext).Assembly);
    }
}
