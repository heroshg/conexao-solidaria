using Campaigns.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Campaigns.Infrastructure.Persistence;

public class CampaignsDbContext(DbContextOptions<CampaignsDbContext> options) : DbContext(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampaignsDbContext).Assembly);
    }
}
