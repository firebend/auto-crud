using System;
using System.Text.RegularExpressions;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Sample.Extensions
{
    public static class MassTransitExtensions
    {
        private static readonly Regex _conStringParser = new Regex(
            "^rabbitmq://([^:]+):(.+)@([^@]+)$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static IServiceCollection AddSampleMassTransit(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var connString = configuration.GetConnectionString("ServiceBus");

            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new Exception("Please configure a service bus connection string for Rabbit MQ");
            }
            
            return serviceCollection.AddMassTransit(bus =>
                {
                    bus.UsingRabbitMq((context, configurator) =>
                    {
                        var match = _conStringParser.Match(connString);

                        var domain = match.Groups[3].Value;
                        var uri = $"rabbitmq://{domain}";
            
                        configurator.Host(new Uri(uri), h =>
                        {
                            h.PublisherConfirmation = true;
                            h.Username(match.Groups[1].Value);
                            h.Password(match.Groups[2].Value);
                        });

                        configurator.Lazy = true;
                        configurator.AutoDelete = true;
                        configurator.PurgeOnStartup = true;
                        
                        context.AddFirebendAutoCrudDomainEventHandlers(configurator, serviceCollection);
                    });
                })
                .AddMassTransitHostedService();
        }
    }
}