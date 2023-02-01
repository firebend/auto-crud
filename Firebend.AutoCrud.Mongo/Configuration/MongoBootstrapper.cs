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
        private static object BootStrapLock { get; set; } = new();
        private static bool _isBootstrapped;

        public static IServiceCollection ConfigureMongoDb(
            this IServiceCollection services,
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

                DoBootstrapping(services, configurator);

                _isBootstrapped = true;

                return services;
            }
        }

        private static void DoBootstrapping(
            this IServiceCollection services,
            IMongoDbConfigurator configurator)
        {
            services.Scan(action => action.FromAssemblies()
                .AddClasses(classes => classes.AssignableTo<IMongoMigration>())
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMongoMigration>()
                .WithTransientLifetime()
            );

            configurator.Configure();

            services.AddHostedService<ConfigureCollectionsHostedService>();
            services.AddHostedService<MongoMigrationHostedService>();
        }
    }
}
