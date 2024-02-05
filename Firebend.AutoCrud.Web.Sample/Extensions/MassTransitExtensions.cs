using System;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

public static class MassTransitExtensions
{

    public static IServiceCollection AddSampleMassTransit(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        bool doMessageLogging)
    {
        var connString = configuration.GetConnectionString("ServiceBus");

        if (string.IsNullOrWhiteSpace(connString))
        {
            throw new Exception("Please configure a service bus connection string for Rabbit MQ");
        }

        return serviceCollection.AddMassTransit(bus =>
        {
            bus.RegisterFirebendAutoCrudDomainEventHandlers(serviceCollection);

            bus.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseNewtonsoftJsonSerializer();
                configurator.ConfigureNewtonsoftJsonDeserializer(x =>
                {
                    x.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    x.TypeNameHandling = TypeNameHandling.All;
                    return x;
                });
                configurator.ConfigureNewtonsoftJsonSerializer(x =>
                {

                    x.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    x.TypeNameHandling = TypeNameHandling.All;
                    return x;
                });

                configurator.Host(connString);

                configurator.Lazy = true;
                configurator.AutoDelete = true;
                configurator.PurgeOnStartup = true;

                context.RegisterFirebendAutoCrudDomainEventHandlerEndPoints(configurator, AutoCrudMassTransitQueueMode.OneQueue);

                if (doMessageLogging)
                {
                    var loggerFactory = context.GetRequiredService<ILoggerFactory>();
                    var logger = new DebugMessageLogger(loggerFactory.CreateLogger<DebugMessageLogger>());
                    configurator.ConnectConsumeAuditObserver(logger);
                    configurator.ConnectSendAuditObservers(logger);
                }
            });
        });
    }
}
