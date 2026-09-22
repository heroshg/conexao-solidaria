using Donations.Domain.Repositories;
using Donations.Infrastructure.Messaging;
using Donations.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Donations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDonationsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Same physical database as Campaigns.Api (db2) — see CLAUDE.md "Arquitetura".
        services.AddDbContext<DonationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CampaignsDb")));

        services.AddScoped<IProcessedDonationRepository, ProcessedDonationRepository>();
        services.AddScoped<ICampaignAmountRepository, CampaignAmountRepository>();
        services.AddScoped<IUnitOfWork, DonationsUnitOfWork>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<DonationReceivedConsumer>();

            busConfigurator.UsingRabbitMq((context, rabbitCfg) =>
            {
                rabbitCfg.Host(configuration.GetConnectionString("RabbitMq"));

                // See MessagingContracts: binds explicitly to the exchange Campaigns.Infrastructure
                // publishes to. The two services each define their own copy of the event class
                // (Campaigns.Application.Events.DonationReceivedEvent — see that file's comment
                // for why the namespace is shared on purpose), so MassTransit's type-URN-based
                // routing matches even without a project reference between them.
                rabbitCfg.ReceiveEndpoint("donations-worker-donation-received", endpoint =>
                {
                    endpoint.Bind(MessagingContracts.DonationReceivedExchange, x => x.ExchangeType = "fanout");

                    // Genuine failures (DB down, etc. — anything ApplyDonationUseCase actually
                    // rethrows) get 5 exponential-backoff retries before MassTransit moves the
                    // message to "donations-worker-donation-received_error" (the DLQ), instead
                    // of losing it or blocking the queue on the first transient failure.
                    endpoint.UseMessageRetry(retryCfg => retryCfg.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));

                    endpoint.ConfigureConsumer<DonationReceivedConsumer>(context);
                });
            });
        });

        return services;
    }
}
