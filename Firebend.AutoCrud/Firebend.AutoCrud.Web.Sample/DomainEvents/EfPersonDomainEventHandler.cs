using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using MassTransit;
using MassTransit.Scoping;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public class EfPersonDomainEventHandler : IEntityAddedDomainEventSubscriber<EfPerson>,
        IEntityUpdatedDomainEventSubscriber<EfPerson>
    {
        private readonly ILogger _logger;
        private ScopedConsumeContextProvider _scoped;

        public EfPersonDomainEventHandler(ILogger<EfPersonDomainEventHandler> logger,
            ScopedConsumeContextProvider scoped)
        {
            _logger = logger;
            _scoped = scoped;
        }

        public Task EntityAddedAsync(EntityAddedDomainEvent<EfPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var modified = domainEvent.Entity;
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);
            
            _logger.LogInformation($"Person Added! Person: {modifiedJson}. Context: {contextJson}");
            _logger.LogInformation($"Catch Phrase: {domainEvent.EventContext.GetCustomContext<CatchPhraseModel>()?.CatchPhrase}");

            if (_scoped.HasContext && _scoped.GetContext().TryGetMessage(out ConsumeContext<EntityAddedDomainEvent<EfPerson>> consumeContext))
            {
                _logger.LogInformation($"From Scope. {consumeContext?.Message?.EventContext?.GetCustomContext<CatchPhraseModel>()?.CatchPhrase}");
            }
            else
            {
                _logger.LogInformation("No scope context");
            }
            
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

            if (_scoped.HasContext && _scoped.GetContext().TryGetMessage(out ConsumeContext<EntityUpdatedDomainEvent<EfPerson>> consumeContext))
            {
                _logger.LogInformation($"From Scope. {consumeContext?.Message?.EventContext?.GetCustomContext<CatchPhraseModel>()?.CatchPhrase}");
            }
            else
            {
                _logger.LogInformation("No scope context");
            }
            return Task.CompletedTask;
        }
    }
}