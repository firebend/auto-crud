using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Sample.DbContexts;
using Firebend.AutoCrud.EntityFramework.Sample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.EntityFramework.Sample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var cancellationToken = new CancellationTokenSource();

            using var host = CreateHostBuilder(args).Build();
            await host.StartAsync(cancellationToken.Token);

            while (!Console.ReadLine()?.Equals("quit", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
            }

            Console.WriteLine("Quitting....");
            cancellationToken.Cancel();
            Console.WriteLine("Done!");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    builder.AddUserSecrets("Firebend.AutoCrud");
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddDbContext<AppDbContext>(opt => { opt.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer")); },
                        ServiceLifetime.Singleton)
                    .UsingEfCrud()
                    .AddEntity<Guid, Person>(person =>
                        person.WithDbContext<AppDbContext>()
                            .WithConnectionString(hostContext.Configuration.GetConnectionString("SqlServer"))
                            .WithDbOptionsProvider<OptionsProvider<Guid, Person>>()
                            .AddCrud(crud => crud.WithCrud().WithSearchHandler<EntitySearchRequest>((persons, request) => persons.Where(x => x.FirstName.StartsWith( request.Search))))
                            .WithRegistration<IEntityReadService<Guid, Person>, PersonReadRepository>()
                    )
                    .AddEntity<Guid, Pet>(pet =>
                        pet.WithDbContext<AppDbContext>()
                            .WithConnectionString(hostContext.Configuration.GetConnectionString("SqlServer"))
                            .WithDbOptionsProvider<OptionsProvider<Guid, Pet>>()
                            .AddCrud(crud => crud.WithCrud().WithSearchHandler<EntitySearchRequest>((pets, request) => pets.Where(x => x.Name.StartsWith(request.Search))))
                    )
                    .Generate();

                services.AddHostedService<SampleHostedService>();
            });
    }
}
