using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions
{
    public static class MassTransitExtensions
    {
        public static void AddFirebendAutoCrudDomainEventHandlers(
            this IBusRegistrationContext busRegistrationContext,
            IBusFactoryConfigurator bus,
            IServiceCollection serviceCollection,
            string receiveEndpointPrefix = null,
            Action<IReceiveEndpointConfigurator> configureReceiveEndpoint = null)
        {
            var listeners = serviceCollection
                .Where(x => IsMessageListener(x.ServiceType))
                .ToArray();
            
            var queueNames = new List<string>();
            
            foreach (var listener in listeners)
            {
                var listenerImplementationType = listener.ImplementationType;
                var serviceType = listener.ServiceType;
                var entity = serviceType.GenericTypeArguments[0];
                var handlerType =  GetHandlerType(entity, serviceType, listenerImplementationType);

                if (handlerType == default || handlerType.type == null)
                {
                    continue;
                }
                
                var genericMessageType = listener.ServiceType.GenericTypeArguments?.FirstOrDefault() ?? listener.ServiceType;
                
                var queueName = GetQueueName(queueNames,
                    receiveEndpointPrefix,
                    genericMessageType,
                    listenerImplementationType,
                    handlerType.desc);


                object ConsumerFactory(Type _)
                {
                    using var serviceScope = busRegistrationContext.CreateScope();

                    try
                    {
                        var handler = serviceScope
                            .ServiceProvider
                            .GetServices(serviceType)
                            .FirstOrDefault(x => x.GetType() == listenerImplementationType);

                        if (handler != null)
                        {
                            var consumer = Activator.CreateInstance(handlerType.type, handler);

                            return consumer;
                        }
                    }
                    catch (Exception ex)
                    {
                        var loggerFactory = serviceScope.ServiceProvider.TryGetService<ILoggerFactory>();
                        var logger = loggerFactory?.CreateLogger(nameof(MassTransitExtensions));
                        logger?.LogError($"Could construct consumer {listenerImplementationType?.FullName}", ex);
                    }

                    return null;
                }

                bus.ReceiveEndpoint(queueName, re =>
                {
                    configureReceiveEndpoint?.Invoke(re);

                    re.Consumer(handlerType.type, ConsumerFactory);
                });
            }
        }

        private static (Type type, string desc) GetHandlerType(Type entity, Type serviceType, Type listenerImplementationType)
        {
            Type handlerType = null;
            string description = null;
            
            if (typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
            {
                handlerType = typeof(MassTransitEntityAddedDomainEventHandler<,>).MakeGenericType(listenerImplementationType, entity);
                description = "EntityAdded";
            }
            else if (typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
            {
                handlerType = typeof(MassTransitEntityUpdatedDomainEventHandler<,>).MakeGenericType(listenerImplementationType, entity);
                description = "EntityUpdated";
            }
            else if (typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(entity) == serviceType)
            {
                handlerType = typeof(MassTransitEntityDeletedDomainEventHandler<,>).MakeGenericType(listenerImplementationType, entity);
                description = "EntityDeleted";
            }

            return (handlerType, description);
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
            if (!serviceType.IsGenericType) return false;

            var args = serviceType.GetGenericArguments();

            if (args.Length != 1 && !args[0].IsClass) return false;

            return typeof(IDomainEventSubscriber).IsAssignableFrom(serviceType);
        }
    }
}