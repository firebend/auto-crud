using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;

public static class MassTransitExtensions
{
    private const string QueuePrefix = "FB_AC_DV_";

    private static List<AutoCrudMassTransitConsumerInfo> _listeners;

    private static List<AutoCrudMassTransitConsumerInfo> GetListeners(IServiceCollection serviceCollection)
    {
        if (_listeners != null && _listeners.Count != 0)
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
        foreach (var listener in GetListeners(serviceCollection))
        {
            busConfigurator.AddConsumer(listener.ConsumerType);
            serviceCollection.TryAddTransient(listener.ServiceDescriptor.ImplementationType!);
        }
    }

    public static void RegisterFirebendAutoCrudDomainEventHandlerEndPoints(
        this IBusRegistrationContext busRegistrationContext,
        IBusFactoryConfigurator bus,
        AutoCrudMassTransitQueueMode queueMode,
        string receiveEndpointPrefix = null,
        Action<IReceiveEndpointConfigurator> configureReceiveEndpoint = null)
    {
        var queues = GetQueues(queueMode, receiveEndpointPrefix, _listeners);

        foreach (var (queueName, consumerInfos) in queues)
        {
            bus.ReceiveEndpoint(queueName, re =>
            {
                foreach (var consumerInfo in consumerInfos)
                {
                    busRegistrationContext.ConfigureConsumer(consumerInfo.ConsumerType, re);
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
                return new() { { prefix, consumerInfos } };

            case AutoCrudMassTransitQueueMode.QueuePerAction:
                return consumerInfos.GroupBy(x => $"{prefix}_{x.EntityActionDescription}")
                    .ToDictionary(x => x.Key,
                        x => x.ToList());

            case AutoCrudMassTransitQueueMode.QueuePerEntity:
                return consumerInfos.GroupBy(x => CleanseQueueName($"{prefix}_{x.EntityType.Name}"))
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
                    dictionary.Add(queueName, [consumerInfo]);
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

    private static string CleanseQueueName(string queueName) => queueName
        .Replace("`1", null)
        .Replace("`2", null)
        .Replace("`3", null)
        .Replace("`4", null)
        .Replace("`5", null)
        .Replace("`6", null)
        .Replace("`7", null)
        .Replace("`8", null)
        .Replace("`9", null)
        .Replace("`10", null)
        .Replace("<", string.Empty)
        .Replace(">", string.Empty)
        .Replace(",", string.Empty)
        .Replace(" ", string.Empty);

    private static string GetQueueName(ICollection<string> queueNames,
        string receiveEndpointPrefix,
        Type genericMessageType,
        MemberInfo listenerImplementationType,
        string handlerTypeDesc)
    {
        var sb = new List<string>();

        if (!string.IsNullOrWhiteSpace(receiveEndpointPrefix))
        {
            sb.Add(receiveEndpointPrefix);
            sb.Add("-");
        }

        sb.Add(genericMessageType.Name);

        if (!string.IsNullOrWhiteSpace(listenerImplementationType?.Name))
        {
            sb.Add("_");
            sb.Add(listenerImplementationType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? listenerImplementationType.Name);
        }

        foreach (var genericTypeArgument in genericMessageType.GenericTypeArguments)
        {
            sb.Add("_");
            sb.Add(genericTypeArgument.Name);
        }

        sb.Add("_");
        sb.Add(handlerTypeDesc);

        var sbBuilt = string.Join(string.Empty, sb);

        if (string.IsNullOrWhiteSpace(sbBuilt))
        {
            throw new Exception("Error building queue name");
        }

        var queueName = CleanseQueueName(sbBuilt);

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
