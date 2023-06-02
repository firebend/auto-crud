using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using MassTransit;
using MassTransit.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.Extensions
{
    public static class MassTransitExtensions
    {

        public static IServiceCollection AddSampleMassTransit(this IServiceCollection serviceCollection, IConfiguration configuration)
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

                    var loggerFactory = context.GetRequiredService<ILoggerFactory>();
                    var logger = new DebugMessageLogger(loggerFactory.CreateLogger<DebugMessageLogger>());
                    configurator.ConnectConsumeAuditObserver(logger);
                    configurator.ConnectSendAuditObservers(logger);
                });
            });
        }
    }

    public class DebugMessageLogger : IMessageAuditStore
    {
        private readonly ILogger<DebugMessageLogger> _logger;

        public DebugMessageLogger(ILogger<DebugMessageLogger> logger)
        {
            _logger = logger;
        }

        public Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
            {
                return Task.CompletedTask;
            }

            _logger.LogDebug(
                "{Action} Message Bus {@Message} with {@Context} {@Payload}",
                metadata.ContextType, message, metadata, JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

            return Task.CompletedTask;
        }
    }
}
