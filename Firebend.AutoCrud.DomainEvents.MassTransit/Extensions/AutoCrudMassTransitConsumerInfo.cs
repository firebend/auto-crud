using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;

public record AutoCrudMassTransitConsumerInfo
{
    public Type EntityType { get; }

    public Type DomainEventType { get; }

    public Type ConsumerType { get; }

    public string EntityActionDescription { get; }

    public ServiceDescriptor ServiceDescriptor { get; }

    public Type MessageType => ServiceDescriptor?.ServiceType.GenericTypeArguments.FirstOrDefault() ?? ServiceDescriptor?.ServiceType;

    public AutoCrudMassTransitConsumerInfo(ServiceDescriptor serviceDescriptor)
    {
        var listenerImplementationType = serviceDescriptor.ImplementationType;
        var serviceType = serviceDescriptor.ServiceType;
        var entity = serviceType.GenericTypeArguments[0];

        Type consumerType = null;
        string description = null;
        Type domainEventType = null;

        if (typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
        {
            consumerType = typeof(MassTransitEntityAddedDomainEventHandler<,>).MakeGenericType(listenerImplementationType!, entity);
            description = "EntityAdded";
            domainEventType = typeof(EntityAddedDomainEvent<>).MakeGenericType(entity);
        }
        else if (typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
        {
            consumerType = typeof(MassTransitEntityUpdatedDomainEventHandler<,>).MakeGenericType(listenerImplementationType!, entity);
            description = "EntityUpdated";
            domainEventType = typeof(EntityUpdatedDomainEvent<>).MakeGenericType(entity);
        }
        else if (typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
        {
            consumerType = typeof(MassTransitEntityDeletedDomainEventHandler<,>).MakeGenericType(listenerImplementationType!, entity);
            description = "EntityDeleted";
            domainEventType = typeof(EntityDeletedDomainEvent<>).MakeGenericType(entity);
        }

        ConsumerType = consumerType;
        EntityType = entity;
        DomainEventType = domainEventType;
        EntityActionDescription = description;
        ServiceDescriptor = serviceDescriptor;
    }
}
