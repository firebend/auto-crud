using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Conventions;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Filtering;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.AutoCrud.Web.Sample.Ordering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.Web.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    if (hostingContext.HostingEnvironment.IsDevelopment()) builder.AddUserSecrets("Firebend.AutoCrud");
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.UsingMongoCrud(hostContext.Configuration.GetConnectionString("Mongo"))
                        .AddBuilder<MongoPerson, Guid>(person =>
                            person.WithDefaultDatabase("Samples")
                                .WithCollection("People")
                                .WithCrud()
                                .WithFullTextSearch()
                                .UsingControllers()
                                .WithAllControllers(true)
                                .WithOpenApiGroupName("The Beautiful Mongo People")
                                .AsEntityBuilder()
                        ).Generate()
                        .UsingEfCrud()
                        .AddBuilder<EfPerson, Guid>(person =>
                            person.WithDbContext<PersonDbContext>()
                                .WithCrud()
                                .WithOrderBy<EfPersonOrder>()
                                .AsBuilder<EntityFrameworkEntityBuilder>()
                                .WithSearchFilter<EfPersonFilter>()
                                .WithDomainEventPublisherServiceProvider()
                                .WithDomainEventEntityAddedSubscriber<EntityFrameworkEntityBuilder, EfPersonDomainEventSubscriber>()
                                .WithDomainEventEntityUpdatedSubscriber<EntityFrameworkEntityBuilder, EfPersonDomainEventSubscriber>()
                                .WithElasticPool(manager =>
                                {
                                    manager.ConnectionString = hostContext.Configuration.GetConnectionString("Elastic");
                                    manager.MapName = hostContext.Configuration["Elastic:MapName"];
                                    manager.Server = hostContext.Configuration["Elastic:ServerName"];
                                    manager.ElasticPoolName = hostContext.Configuration["Elastic:PoolName"];
                                })
                                .WithShardKeyProvider<SampleKeyProvider>()
                                .WithShardDbNameProvider<SampleDbNameProvider>()
                                .UsingControllers()
                                .WithAllControllers(true)
                                .WithOpenApiGroupName("The Beautiful Sql People")
                                .AsEntityBuilder())
                        .Generate()
                        .AddRouting()
                        .AddSwaggerGen(opt =>
                        {
                            opt.TagActionsBy(FirebendAutoCrudSwaggerGenTagger.TagActionsBy);
                        })
                        .AddControllers()
                        .AddNewtonsoftJson()
                        .ConfigureApplicationPartManager(
                            manager => manager.FeatureProviders.Insert(0, new FirebendAutoCrudControllerConvention(services)));
                });
        }
    }
}