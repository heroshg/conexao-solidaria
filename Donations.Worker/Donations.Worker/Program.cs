using Donations.Application.UseCases;
using Donations.Infrastructure;
using Donations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDonationsInfrastructure(builder.Configuration);
builder.Services.AddScoped<ApplyDonationUseCase>();

var campaignsDbConnectionString = builder.Configuration.GetConnectionString("CampaignsDb")!;
builder.Services.AddHealthChecks()
    .AddNpgSql(campaignsDbConnectionString, name: "campaigns-db");

var app = builder.Build();

// No Controllers here on purpose — Donations.Worker has no business HTTP API, only
// observability endpoints. See CLAUDE.md "Arquitetura de código".
app.UseHttpMetrics();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DonationsDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
