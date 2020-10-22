using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public class EfPersonDomainEventSubscriber : IEntityAddedDomainEventSubscriber<EfPerson>,
        IEntityUpdatedDomainEventSubscriber<EfPerson>
    {
        private readonly ILogger _logger;

        public EfPersonDomainEventSubscriber(ILogger<EfPersonDomainEventSubscriber> logger)
        {
            _logger = logger;
        }

        public Task EntityAddedAsync(EntityAddedDomainEvent<EfPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var entity = domainEvent.Entity;
            _logger.LogInformation($"Person Added! {JsonConvert.SerializeObject(entity)}");
            return Task.CompletedTask;
        }

        public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<EfPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var original = domainEvent.Previous;
            var modified = domainEvent.Modified;
            
            _logger.LogInformation($"Person Updated! {JsonConvert.SerializeObject(original)} {JsonConvert.SerializeObject(modified)}");
            return Task.CompletedTask;
        }
    }
}