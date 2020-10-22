using System;
using System.Linq;
using System.Text;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.Interfaces;
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
            var listenerType = typeof(IMassTransitDomainEventHandler<>);
            var messageType = typeof(DomainEventBase);

            var listeners = serviceCollection
                .Where(x => IsMessageListener(x.ServiceType, listenerType, messageType))
                .ToArray();
            
            foreach (var listener in listeners)
            {
                var genericMessageType = listener.ServiceType.GenericTypeArguments?.FirstOrDefault() ?? listener.ServiceType;
                var listenerImplementationType = listener.ImplementationType;
                var serviceType = listener.ServiceType;
                var queueName = GetQueueName(receiveEndpointPrefix, genericMessageType, listenerImplementationType);

                bus.ReceiveEndpoint(queueName, re =>
                {
                    configureReceiveEndpoint?.Invoke(re);

                    re.Consumer(listenerImplementationType, _  =>
                    {
                        using var serviceScope = busRegistrationContext.CreateScope();
                        
                        try
                        {
                            
                            var service = serviceScope.ServiceProvider.GetService(serviceType);

                            if (service is IConsumer consumer)
                            {
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
                    });
                });
            }
        }

        private static string GetQueueName(string receiveEndpointPrefix, Type genericMessageType, Type listenerImplementationType)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(receiveEndpointPrefix))
            {
                sb.Append(receiveEndpointPrefix);
                sb.Append("-");
            }

            sb.Append(genericMessageType.Name.Replace("`1", null));

            if (!string.IsNullOrWhiteSpace(listenerImplementationType?.Name))
            {
                sb.Append("_");
                sb.Append(listenerImplementationType.Name.Replace("`1", null));
            }

            return sb.ToString();
        }

        private static bool IsMessageListener(Type serviceType, Type listenerType, Type messageType)
        {
            if (!serviceType.IsGenericType) return false;
            
            var args = serviceType.GetGenericArguments();

            if (args.Length != 1) return false;
                
            if (!messageType.IsAssignableFrom(args[0])) return false;
                
            var isListener = listenerType.MakeGenericType(args).IsAssignableFrom(serviceType);

            return isListener;
            
        }
    }
}