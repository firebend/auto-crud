using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Tenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.Web.Sample
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    builder.AddUserSecrets("Firebend.AutoCrud");
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddScoped<ITenantEntityProvider<int>, SampleTenantProvider>()
                    .UsingMongoCrud(hostContext.Configuration.GetConnectionString("Mongo"), mongo =>
                    {
                        mongo.AddMongoPerson();
                    })
                    .UsingEfCrud(ef =>
                    {
                        ef.AddEfPerson(hostContext.Configuration)
                            .AddEfPets(hostContext.Configuration)
                            .WithDomainEventContextProvider<SampleDomainEventContextProvider>();
                    })
                    .AddSampleMassTransit(hostContext.Configuration)
                    .AddRouting()
                    .AddSwaggerGen()
                    .AddControllers()
                    .AddNewtonsoftJson()
                    .AddFirebendAutoCrudWeb(services);

                services.Configure<ApiBehaviorOptions>(o => o.SuppressInferBindingSourcesForParameters = true);
            });
    }
}
