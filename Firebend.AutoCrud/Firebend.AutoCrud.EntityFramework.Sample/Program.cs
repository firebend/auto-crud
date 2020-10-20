using System;
using System.Threading;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
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
        private static void Main(string[] args)
        {
            var cancellationToken = new CancellationTokenSource();

            using var host = CreateHostBuilder(args).Build();
            host.StartAsync(cancellationToken.Token)
                .ContinueWith(task => { Console.WriteLine("Sample complete. type 'quit' to exit."); }, cancellationToken.Token);

            while (!Console.ReadLine().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
            }

            Console.WriteLine("Quiting....");
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
                    services.AddDbContext<AppDbContext>(opt => { opt.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer")); },
                            ServiceLifetime.Singleton)
                        .UsingEfCrud()
                        .AddBuilder(new EntityFrameworkEntityBuilder<Guid, Person>(),  person =>
                            person.AddCrud(crud => crud.WithCrud())
                                .WithDbContext<AppDbContext>()
                                .WithRegistration<IEntityReadService<Guid, Person>, PersonReadRepository>()
                            )
                        .AddBuilder<Pet, Guid>(pet =>
                            pet.AddCrud(crud => crud.WithCrud())
                                .WithDbContext<AppDbContext>()
                            )
                        .Generate();

                    services.AddHostedService<SampleHostedService>();
                });
        }
    }
}