using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Conventions;
using Firebend.AutoCrud.Web.Sample.Models;
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    if (hostingContext.HostingEnvironment.IsDevelopment()) builder.AddUserSecrets("Firebend.AutoCrud");
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.UsingMongoCrud(hostContext.Configuration.GetConnectionString("Mongo"))
                        .AddBuilder<Person, Guid>(person =>
                            person.WithDefaultDatabase("Samples")
                                .WithCollection("People")
                                .WithCrud()
                                .WithFullTextSearch()
                                .UsingControllers()
                                .WithOpenApiGroupName("The Beautiful People")
                                .WithAllControllers(true)
                                .AsEntityBuilder()
                        ).Generate()
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
                        .ConfigureApplicationPartManager(
                            manager => manager.FeatureProviders.Insert(0, new FirebendAutoCrudControllerConvention(services)));
                });
    }
}
