using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Generator.Implementations;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Sample.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.Mongo.Sample
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var builder = new MongoDbEntityBuilder(new DynamicClassGenerator());
                    builder.ForEntity<MongoDbEntityBuilder, Person, Guid>()
                        .WithDatabase("Samples")
                        .WithCollection("People")
                        .WithCrud();

                    var crudGenerator = new EntityCrudGenerator(new DynamicClassGenerator());
                    
                    crudGenerator.Generate(services, builder);
                    
                    services.AddHostedService<SampleHostedService>();
                });
    }
}