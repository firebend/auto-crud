﻿using System;
using System.Threading;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Sample.Models;
using Firebend.AutoCrud.Mongo.Sample.Tenant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.Mongo.Sample
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var cancellationToken = new CancellationTokenSource();

            using var host = CreateHostBuilder(args).Build();
            host.StartAsync(cancellationToken.Token)
                .ContinueWith(task => { Console.WriteLine("Sample complete. type 'quit' to exit."); }, cancellationToken.Token);

            while (!Console.ReadLine().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
            }

            Console.WriteLine("Quitting....");
            cancellationToken.Cancel();
            Console.WriteLine("Done!");
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    if (hostingContext.HostingEnvironment.IsDevelopment()) builder.AddUserSecrets("Firebend.AutoCrud");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddScoped<ITenantEntityProvider<int>, SampleTenantProvider>()
                        .UsingMongoCrud(hostContext.Configuration.GetConnectionString("Mongo"))
                        .AddEntity<Guid, Person>(person => 
                            person.WithDefaultDatabase("Samples")
                                .WithCollection("People")
                                .WithFullTextSearch()
                                .AddCrud()
                                .WithRegistration<IEntityReadService<Guid, Person>, PersonReadRepository>()
                        ).Generate();

                    services.AddHostedService<SampleHostedService>();
                });
        }
    }
}