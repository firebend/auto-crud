using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public class EfPersonDomainEventSubscriber : IEntityAddedDomainEventSubscriber<EfPerson>
    {
        private readonly ILogger _logger;

        public EfPersonDomainEventSubscriber(ILogger<EfPersonDomainEventSubscriber> logger)
        {
            _logger = logger;
        }

        public Task EntityAddedAsync(EfPerson entity, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Person Changed! {JsonConvert.SerializeObject(entity)}");

            return Task.CompletedTask;
        }
    }
}