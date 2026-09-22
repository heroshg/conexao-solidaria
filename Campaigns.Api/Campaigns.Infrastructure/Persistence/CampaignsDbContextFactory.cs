using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Campaigns.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef migrations add` run against this project without the full host/DI.</summary>
public class CampaignsDbContextFactory : IDesignTimeDbContextFactory<CampaignsDbContext>
{
    public CampaignsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CAMPAIGNS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=campaigns;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<CampaignsDbContext>()
            .UseNpgsql(connectionString);

        return new CampaignsDbContext(optionsBuilder.Options);
    }
}
