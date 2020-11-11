using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions
{

    public static class MassTransitExtensions
    {
        private static ServiceDescriptor[] _listeners;

        private static ServiceDescriptor[] GetListeners(IServiceCollection serviceCollection)
        {
            if (_listeners != null && _listeners.Any())
            {
                return _listeners;
            }

            var listeners = serviceCollection
                .Where(x => IsMessageListener(x.ServiceType))
                .ToArray();

            _listeners = listeners;

            return listeners;
        }

        public static void RegisterFirebendAutoCrudDomainEventHandlers(this IServiceCollectionBusConfigurator busConfigurator, IServiceCollection serviceCollection)
        {
            var addConsumer = typeof(MassTransitExtensions).GetMethod(nameof(AddConsumer),
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Static);

            if (addConsumer == null)
            {
                return;
            }

            foreach (var listener in GetListeners(serviceCollection))
            {
                var (consumerType, domainEventType, _) = GetConsumerInfo(listener);

                addConsumer
                    .MakeGenericMethod(listener.ImplementationType, domainEventType, consumerType)
                    .Invoke(null, new object[] { busConfigurator, serviceCollection });
            }
        }

        public static void RegisterFirebendAutoCrudeDomainEventHandlerEndPoints(
            this IBusRegistrationContext busRegistrationContext,
            IBusFactoryConfigurator bus,
            IServiceCollection serviceCollection,
            string receiveEndpointPrefix = null,
            Action<IReceiveEndpointConfigurator> configureReceiveEndpoint = null)
        {
            var listeners = GetListeners(serviceCollection);

            var queueNames = new List<string>();

            var configureConsumer = typeof(MassTransitExtensions).GetMethod(nameof(ConfigureConsumer),
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Static);

            if (configureConsumer == null)
            {
                return;
            }

            foreach (var listener in listeners)
            {
                var (consumerType, domainEventType, desc) = GetConsumerInfo(listener);

                if (consumerType == null || domainEventType == null)
                {
                    continue;
                }

                var genericMessageType = listener.ServiceType.GenericTypeArguments.FirstOrDefault() ?? listener.ServiceType;

                var queueName = GetQueueName(queueNames,
                    receiveEndpointPrefix,
                    genericMessageType,
                    listener.ImplementationType,
                    desc);

                bus.ReceiveEndpoint(queueName, re =>
                {
                    configureConsumer.MakeGenericMethod(listener.ImplementationType, domainEventType, consumerType)
                        .Invoke(null, new object[] { busRegistrationContext, re });

                    configureReceiveEndpoint?.Invoke(re);
                });
            }

            _listeners = null;
        }

        private static void AddConsumer<TDomainEventHandler, TDomainEvent, TDomainEventConsumer>(IServiceCollectionBusConfigurator busConfigurator,
            IServiceCollection serviceCollection)
            where TDomainEvent : DomainEventBase
            where TDomainEventHandler : class, IDomainEventSubscriber
            where TDomainEventConsumer : AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler>
        {
            serviceCollection.TryAddTransient<TDomainEventHandler>();
            busConfigurator.AddConsumer<TDomainEventConsumer>();
        }

        private static void ConfigureConsumer<TDomainEventHandler, TDomainEvent, TDomainEventConsumer>(
            IRegistration context,
            IReceiveEndpointConfigurator receiveEndpointConfigurator)
            where TDomainEvent : DomainEventBase
            where TDomainEventHandler : class, IDomainEventSubscriber
            where TDomainEventConsumer : AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler>
        {
            context.ConfigureConsumer<TDomainEventConsumer>(receiveEndpointConfigurator);
        }

        private static (Type consumerType, Type domainEventType, string desc) GetConsumerInfo(ServiceDescriptor serviceDescriptor)
        {
            var listenerImplementationType = serviceDescriptor.ImplementationType;
            var serviceType = serviceDescriptor.ServiceType;
            var entity = serviceType.GenericTypeArguments[0];

            Type consumerType = null;
            string description = null;
            Type domainEventType = null;

            if (typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
            {
                consumerType = typeof(MassTransitEntityAddedDomainEventHandler<,>).MakeGenericType(listenerImplementationType, entity);
                description = "EntityAdded";
                domainEventType = typeof(EntityAddedDomainEvent<>).MakeGenericType(entity);
            }
            else if (typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
            {
                consumerType = typeof(MassTransitEntityUpdatedDomainEventHandler<,>).MakeGenericType(listenerImplementationType, entity);
                description = "EntityUpdated";
                domainEventType = typeof(EntityUpdatedDomainEvent<>).MakeGenericType(entity);
            }
            else if (typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
            {
                consumerType = typeof(MassTransitEntityDeletedDomainEventHandler<,>).MakeGenericType(listenerImplementationType, entity);
                description = "EntityDeleted";
                domainEventType = typeof(EntityDeletedDomainEvent<>).MakeGenericType(entity);
            }

            return (consumerType, domainEventType, description);
        }

        private static string GetQueueName(ICollection<string> queueNames,
            string receiveEndpointPrefix,
            MemberInfo genericMessageType,
            MemberInfo listenerImplementationType,
            string handlerTypeDesc)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(receiveEndpointPrefix))
            {
                sb.Append(receiveEndpointPrefix);
                sb.Append("-");
            }

            sb.Append(genericMessageType.Name);

            if (!string.IsNullOrWhiteSpace(listenerImplementationType?.Name))
            {
                sb.Append("_");
                sb.Append(listenerImplementationType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? listenerImplementationType.Name);
            }

            sb.Append("_");
            sb.Append(handlerTypeDesc);

            var queueName = sb.ToString().Replace("`1", null).Replace("`2", null);

            while (queueNames.Contains(queueName))
            {
                var last = queueName.Last();
                var iteration = char.IsDigit(last) ? int.Parse(last.ToString()) : 0;
                iteration++;
                queueName += iteration;
            }

            queueNames.Add(queueName);

            return queueName;
        }

        private static bool IsMessageListener(Type serviceType)
        {
            if (!serviceType.IsGenericType)
                return false;

            var args = serviceType.GetGenericArguments();

            if (args.Length != 1 && !args[0].IsClass)
                return false;

            return typeof(IDomainEventSubscriber).IsAssignableFrom(serviceType);
        }
    }
}
