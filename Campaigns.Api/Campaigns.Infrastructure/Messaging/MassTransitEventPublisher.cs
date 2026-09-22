using Campaigns.Application.Abstractions;
using MassTransit;

namespace Campaigns.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class =>
        publishEndpoint.Publish(@event, cancellationToken);
}
