using System;
using Firebend.AutoCrud.Mongo.HostedServices;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Scrutor;

namespace Firebend.AutoCrud.Mongo
{
    public static class MongoBootstrapper
    {
        public static IServiceCollection ConfigureMongoDb(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment,
            string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                throw new ArgumentNullException(nameof(connectionStringName));
            }
            
            services.Scan(action => action.FromAssemblies()
                .AddClasses(classes => classes.AssignableTo<IMongoMigration>())
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMongoMigration>()
                .WithTransientLifetime()
            );

            var connString = configuration.GetConnectionString(connectionStringName);

            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new Exception($"Mongo connection string not found. Name : {connectionStringName} ");
            }

            var mongoUrl = new MongoUrl(connString);

            MongoDbConfigurator.Configure();

            var isDev = environment.IsDevelopment();

            services.AddScoped<IMongoClient>(x =>
            {
                var logger = x.GetService<ILogger<MongoClient>>();

                var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);

                if (isDev)
                {
                    mongoClientSettings.ClusterConfigurator = cb =>
                    {
                        cb.Subscribe<CommandStartedEvent>(e =>
                            logger.LogDebug("MONGO: {CommandName} - {Command}", e.CommandName, e.Command.ToJson()));

                        cb.Subscribe<CommandSucceededEvent>(e =>
                            logger.LogDebug("SUCCESS: {CommandName}({Duration}) - {Reply}", e.CommandName, e.Duration,
                                e.Reply.ToJson()));

                        cb.Subscribe<CommandFailedEvent>(e =>
                            logger.LogError("ERROR: {CommandName}({Duration}) - {Error}", e.CommandName, e.Duration,
                                e.Failure));
                    };
                }

                return new MongoClient(mongoClientSettings);
            });

            services.AddHostedService<ConfigureCollectionsHostedService>();

            services.AddHostedService<MongoMigrationHostedService>();

            return services;
        }
    }
}