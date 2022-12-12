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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions
{
    public static class MassTransitExtensions
    {
        private const string QueuePrefix = "FB_AC_DV_";

        private static List<AutoCrudMassTransitConsumerInfo> _listeners;

        private static IEnumerable<AutoCrudMassTransitConsumerInfo> GetListeners(IServiceCollection serviceCollection)
        {
            if (_listeners != null && _listeners.Any())
            {
                return _listeners;
            }

            var listeners = serviceCollection
                .Where(x => IsMessageListener(x.ServiceType))
                .Select(x => new AutoCrudMassTransitConsumerInfo(x))
                .ToList();

            _listeners = listeners;

            return listeners;
        }

        public static void RegisterFirebendAutoCrudDomainEventHandlers(this IBusRegistrationConfigurator busConfigurator,
            IServiceCollection serviceCollection)
        {
            var addConsumer = typeof(MassTransitExtensions).GetMethod(nameof(AddConsumer),
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Static);

            if (addConsumer == null)
            {
                return;
            }

            foreach (var listener in GetListeners(serviceCollection))
            {
                addConsumer
                    .MakeGenericMethod(listener.ServiceDescriptor.ImplementationType, listener.DomainEventType, listener.ConsumerType)
                    .Invoke(null, new object[] { busConfigurator, serviceCollection });
            }
        }

        public static void RegisterFirebendAutoCrudDomainEventHandlerEndPoints(
            this IBusRegistrationContext busRegistrationContext,
            IBusFactoryConfigurator bus,
            AutoCrudMassTransitQueueMode queueMode,
            string receiveEndpointPrefix = null,
            Action<IReceiveEndpointConfigurator> configureReceiveEndpoint = null)
        {
            var configureConsumer = typeof(MassTransitExtensions).GetMethod(nameof(ConfigureConsumer),
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Static);

            if (configureConsumer == null)
            {
                return;
            }

            var queues = GetQueues(queueMode, receiveEndpointPrefix, _listeners);

            foreach (var (queueName, consumerInfos) in queues)
            {
                bus.ReceiveEndpoint(queueName, re =>
                {
                    foreach (var consumerInfo in consumerInfos)
                    {
                        configureConsumer.MakeGenericMethod(consumerInfo.ServiceDescriptor.ImplementationType,
                                consumerInfo.DomainEventType,
                                consumerInfo.ConsumerType)
                            .Invoke(null, new object[]
                            {
                                busRegistrationContext,
                                re
                            });
                    }
                    configureReceiveEndpoint?.Invoke(re);
                });
            }

            _listeners.Clear();
            _listeners = null;
        }

        private static Dictionary<string, List<AutoCrudMassTransitConsumerInfo>> GetQueues(AutoCrudMassTransitQueueMode queueMode,
            string receiveEndpointPrefix,
            List<AutoCrudMassTransitConsumerInfo> consumerInfos)
        {
            if (queueMode == AutoCrudMassTransitQueueMode.Unknown)
            {
                throw new ArgumentException("Queue mode is unknown", nameof(queueMode));
            }

            var prefix = QueuePrefix;

            if (!string.IsNullOrWhiteSpace(receiveEndpointPrefix))
            {
                prefix = $"{prefix}_{receiveEndpointPrefix}";
            }

            switch (queueMode)
            {
                case AutoCrudMassTransitQueueMode.OneQueue:
                    return new Dictionary<string, List<AutoCrudMassTransitConsumerInfo>> { { prefix, consumerInfos } };

                case AutoCrudMassTransitQueueMode.QueuePerAction:
                    return consumerInfos.GroupBy(x => $"{prefix}_{x.EntityActionDescription}")
                        .ToDictionary(x => x.Key,
                            x => x.ToList());

                case AutoCrudMassTransitQueueMode.QueuePerEntity:
                    return consumerInfos.GroupBy(x => $"{prefix}_{x.EntityType.Name}")
                        .ToDictionary(x => x.Key, x => x.ToList());

                case AutoCrudMassTransitQueueMode.QueuePerEntityAction:
                {
                    var dictionary = new Dictionary<string, List<AutoCrudMassTransitConsumerInfo>>();
                    var queueNames = new List<string>();

                    foreach (var consumerInfo in consumerInfos)
                    {
                        var queueName = GetQueueName(queueNames,
                            prefix,
                            consumerInfo.MessageType,
                            consumerInfo.ServiceDescriptor.ImplementationType,
                            consumerInfo.EntityActionDescription
                        );
                        dictionary.Add(queueName, new List<AutoCrudMassTransitConsumerInfo> { consumerInfo });
                    }

                    return dictionary;
                }
                case AutoCrudMassTransitQueueMode.Unknown:
                    throw new ArgumentException($"{nameof(AutoCrudMassTransitQueueMode)} {nameof(AutoCrudMassTransitQueueMode.Unknown)} is not a valid mode.",
                        nameof(queueMode));
                default:
                    throw new ArgumentException($"{nameof(AutoCrudMassTransitQueueMode)} is not a valid mode.",
                        nameof(queueMode));
            }
        }

        private static void AddConsumer<TDomainEventHandler, TDomainEvent, TDomainEventConsumer>(IRegistrationConfigurator busConfigurator,
            IServiceCollection serviceCollection)
            where TDomainEvent : DomainEventBase
            where TDomainEventHandler : class, IDomainEventSubscriber
            where TDomainEventConsumer : AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler>
        {
            serviceCollection.TryAddTransient<TDomainEventHandler>();
            busConfigurator.AddConsumer<TDomainEventConsumer>();
        }

        private static void ConfigureConsumer<TDomainEventHandler, TDomainEvent, TDomainEventConsumer>(
            IRegistrationContext context,
            IReceiveEndpointConfigurator receiveEndpointConfigurator)
            where TDomainEvent : DomainEventBase
            where TDomainEventHandler : class, IDomainEventSubscriber
            where TDomainEventConsumer : AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler>
            => context.ConfigureConsumer<TDomainEventConsumer>(receiveEndpointConfigurator);

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
                sb.Append('-');
            }

            sb.Append(genericMessageType.Name);

            if (!string.IsNullOrWhiteSpace(listenerImplementationType?.Name))
            {
                sb.Append('_');
                sb.Append(listenerImplementationType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? listenerImplementationType.Name);
            }

            sb.Append('_');
            sb.Append(handlerTypeDesc);
            var sbBuilt = sb.ToString();

            if (string.IsNullOrWhiteSpace(sbBuilt))
            {
                throw new Exception("Error building queue name");
            }

            var queueName = sbBuilt.Replace("`2", null);

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
            {
                return false;
            }

            var args = serviceType.GetGenericArguments();

            if (args.Length != 1 && !args[0].IsClass)
            {
                return false;
            }

            return typeof(IDomainEventSubscriber).IsAssignableFrom(serviceType);
        }
    }
}
