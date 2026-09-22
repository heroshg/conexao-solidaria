using Campaigns.Application.Events;
using Donations.Application.UseCases;
using MassTransit;

namespace Donations.Infrastructure.Messaging;

public sealed class DonationReceivedConsumer(ApplyDonationUseCase applyDonationUseCase) : IConsumer<DonationReceivedEvent>
{
    public Task Consume(ConsumeContext<DonationReceivedEvent> context) =>
        applyDonationUseCase.ExecuteAsync(context.Message, context.CancellationToken);
}
