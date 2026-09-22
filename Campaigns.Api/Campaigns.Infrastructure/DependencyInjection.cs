using Campaigns.Application.Abstractions;
using Campaigns.Application.Events;
using Campaigns.Domain.Repositories;
using Campaigns.Infrastructure.Messaging;
using Campaigns.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campaigns.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCampaignsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CampaignsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CampaignsDb")));

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IUnitOfWork, CampaignsUnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.UsingRabbitMq((_, rabbitCfg) =>
            {
                rabbitCfg.Host(configuration.GetConnectionString("RabbitMq"));

                // See MessagingContracts: named explicitly so Donations.Infrastructure's
                // independently-defined copy of this event binds to the same exchange.
                rabbitCfg.Message<DonationReceivedEvent>(
                    x => x.SetEntityName(MessagingContracts.DonationReceivedExchange));
                rabbitCfg.Publish<DonationReceivedEvent>(x => x.ExchangeType = "fanout");
            });
        });

        return services;
    }
}
