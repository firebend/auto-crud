using System;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.HostedServices;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Firebend.AutoCrud.Mongo.Configuration
{
    public static class MongoBootstrapper
    {
        private static readonly object BootStrapLock = new();
        private static bool _isBootstrapped;

        public static IServiceCollection ConfigureMongoDb(
            this IServiceCollection services,
            string connectionString,
            bool enableCommandLogging,
            IMongoDbConfigurator configurator)
        {
            if (_isBootstrapped)
            {
                return services;
            }

            lock (BootStrapLock)
            {
                if (_isBootstrapped)
                {
                    return services;
                }

                DoBootstrapping(services, connectionString, enableCommandLogging, configurator);

                _isBootstrapped = true;

                return services;
            }
        }

        private static void DoBootstrapping(
            this IServiceCollection services,
            string connectionString,
            bool enableCommandLogging,
            IMongoDbConfigurator configurator)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            services.Scan(action => action.FromAssemblies()
                .AddClasses(classes => classes.AssignableTo<IMongoMigration>())
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMongoMigration>()
                .WithTransientLifetime()
            );

            configurator.Configure();
            services.AddSingleton<IMongoClientFactory, MongoClientFactory>();
            services.AddSingleton(provider =>
            {
                var factory = provider.GetService<IMongoClientFactory>();
                var client = factory?.CreateClient(connectionString, enableCommandLogging);
                return client;
            });

            services.AddHostedService<ConfigureCollectionsHostedService>();
            services.AddHostedService<MongoMigrationHostedService>();
        }
    }
}
