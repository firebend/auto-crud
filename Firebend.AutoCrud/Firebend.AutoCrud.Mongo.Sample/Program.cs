using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Generator.Implementations;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Sample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.Mongo.Sample
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            Console.WriteLine("Sample complete. type 'quit' to exit.");
            
            while (Console.ReadLine().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
                
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets("Firebend.AutoCrud");
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var builder = new MongoDbEntityBuilder(new DynamicClassGenerator());
                    
                    builder.ForEntity<MongoDbEntityBuilder, Person, Guid>()
                        .WithDefaultDatabase("Samples")
                        .WithCollection("People")
                        .WithCrud()
                        .Build();

                    var crudGenerator = new EntityCrudGenerator(new DynamicClassGenerator());
                    
                    crudGenerator.Generate(services, builder);

                    services.ConfigureMongoDb(hostContext.Configuration.GetConnectionString("Mongo"),
                        true,
                        new MongoDbConfigurator());
                    
                    services.AddHostedService<SampleHostedService>();
                    
                });
    }
}