using System;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;

public record AutoCrudMassTransitConfigureReceiveEndpointContext(
    IReceiveEndpointConfigurator EndpointConfigurator,
    string QueueName,
    Type ConsumerType,
    Type MessageType
);
