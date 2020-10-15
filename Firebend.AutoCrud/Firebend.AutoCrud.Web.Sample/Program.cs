using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Conventions;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
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
                                .WithDomainEventPublisherServiceProvider()
                                .WithDomainEventEntityAddedSubscriber<EntityFrameworkEntityBuilder, EfPersonDomainEventSubscriber>()
                                .UsingControllers()
                                .WithAllControllers(true)
                                .WithOpenApiGroupName("The Beautiful Sql People")
                                .AsEntityBuilder())
                        .Generate()
                        .AddDbContext<PersonDbContext>(opt =>
                        {
                            opt.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer"));
                        })
                        .AddRouting()
                        .AddSwaggerGen(opt =>
                        {
                            opt.TagActionsBy(x =>
                            {
                                List<string> list;

                                if (x.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
                                {
                                    list = new List<string>
                                    {
                                        controllerDescriptor.ControllerTypeInfo?.GetCustomAttribute<OpenApiGroupNameAttribute>()?.GroupName ??
                                        controllerDescriptor.ControllerTypeInfo?.Namespace?.Split('.')?.Last() ??
                                        x.RelativePath
                                    };
                                }
                                else
                                {
                                    list = new List<string>
                                    {
                                        x.RelativePath
                                    };
                                }

                                return list;
                            });
                        })
                        .AddControllers()
                        .AddNewtonsoftJson()
                        .ConfigureApplicationPartManager(
                            manager => manager.FeatureProviders.Insert(0, new FirebendAutoCrudControllerConvention(services)));
                });
        }
    }
}