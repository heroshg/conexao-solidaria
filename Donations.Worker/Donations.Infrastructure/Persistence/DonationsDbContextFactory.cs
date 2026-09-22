using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Donations.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef migrations add` run against this project without the full host/DI.</summary>
public class DonationsDbContextFactory : IDesignTimeDbContextFactory<DonationsDbContext>
{
    public DonationsDbContext CreateDbContext(string[] args)
    {
        // Same physical database as Campaigns.Api (db2) — see CLAUDE.md "Arquitetura".
        var connectionString = Environment.GetEnvironmentVariable("CAMPAIGNS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=campaigns;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<DonationsDbContext>()
            .UseNpgsql(connectionString);

        return new DonationsDbContext(optionsBuilder.Options);
    }
}
