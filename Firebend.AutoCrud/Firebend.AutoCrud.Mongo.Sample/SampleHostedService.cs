using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Sample.Models;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.Mongo.Sample
{
    public class SampleHostedService : IHostedService
    {
        private CancellationTokenSource _cancellationTokenSource;
        
        private IEntityCreateService<Guid, Person> _createService;

        public SampleHostedService(IEntityCreateService<Guid, Person> createService)
        {
            _createService = createService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await _createService.CreateAsync(new Person
            {
                FirstName = $"First Name -{DateTimeOffset.UtcNow}",
                LastName = $"Last Name -{DateTimeOffset.UtcNow}"

            }, _cancellationTokenSource.Token);

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            
            return Task.CompletedTask;
        }
    }
}