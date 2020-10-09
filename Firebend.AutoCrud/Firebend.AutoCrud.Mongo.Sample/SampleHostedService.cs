using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Sample.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = DnsClient.Internal.ILogger;

namespace Firebend.AutoCrud.Mongo.Sample
{
    public class SampleHostedService : IHostedService
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IEntityCreateService<Guid, Person> _createService;
        private readonly ILogger<SampleHostedService> _logger;

        public SampleHostedService(IServiceProvider serviceProvider, ILogger<SampleHostedService> logger)
        {
            _logger = logger;
            using var scope = serviceProvider.CreateScope();
            _createService = scope.ServiceProvider.GetService<IEntityCreateService<Guid, Person>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            _logger.LogInformation("Starting Sample...");

            var entity = await _createService.CreateAsync(new Person
            {
                FirstName = $"First Name -{DateTimeOffset.UtcNow}",
                LastName = $"Last Name -{DateTimeOffset.UtcNow}"

            }, _cancellationTokenSource.Token);
            
            _logger.LogInformation("Entity Added.");
            
            JsonSerializer.CreateDefault().Serialize(Console.Out, entity);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            
            return Task.CompletedTask;
        }
    }
}