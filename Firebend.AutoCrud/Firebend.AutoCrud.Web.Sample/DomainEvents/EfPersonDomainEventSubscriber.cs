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
            var modified = domainEvent.Entity;
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);
            
            _logger.LogInformation($"Person Added! Person: {modifiedJson}. Context: {contextJson}");
            _logger.LogInformation($"Catch Phrase: {domainEvent.EventContext.GetCustomContext<CatchPhraseModel>()?.CatchPhrase}");
            
            return Task.CompletedTask;
        }

        public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<EfPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var original = domainEvent.Previous;
            var modified = domainEvent.Modified;
            var originalJson = JsonConvert.SerializeObject(original, Formatting.Indented);
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);
            
            _logger.LogInformation($"Person Updated! Original: {originalJson}. Modified: {modifiedJson}. Context: {contextJson}");
            _logger.LogInformation($"Catch Phrase: {domainEvent.EventContext.GetCustomContext<CatchPhraseModel>()?.CatchPhrase}");

            return Task.CompletedTask;
        }
    }
}