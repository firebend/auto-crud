using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using MassTransit;
using MassTransit.Scoping;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public partial class MongoPersonDomainEventHandler : BaseDisposable, IEntityAddedDomainEventSubscriber<MongoTenantPerson>,
        IEntityUpdatedDomainEventSubscriber<MongoTenantPerson>
    {
        [LoggerMessage(EventId = 0, Message = "Mongo Person Added! Person: {modifiedJson}. Context: {contextJson}", Level = LogLevel.Debug)]
        public static partial void LogPersonAdded(ILogger logger, string modifiedJson, string contextJson);

        [LoggerMessage(EventId = 2, Message = "Mongo Catch Phrase: {catchPhrase}", Level = LogLevel.Debug)]
        public static partial void LogCatchPhrase(ILogger logger, string catchPhrase);

        [LoggerMessage(EventId = 3, Message = "Mongo Catch Phrase From Scope: {catchPhrase}", Level = LogLevel.Debug)]
        public static partial void LogCatchPhraseFromScope(ILogger logger, string catchPhrase);

        [LoggerMessage(EventId = 4, Message = "Mongo No Scope Context", Level = LogLevel.Debug)]
        public static partial void LogNoScopeContext(ILogger logger);

        [LoggerMessage(EventId = 5, Message = "Mongo Person Updated! Original: {originalJson}. Modified: {modifiedJson}. Context: {contextJson}",
            Level = LogLevel.Debug)]
        public static partial void LogPersonUpdated(ILogger logger, string originalJson, string modifiedJson, string contextJson);

        private readonly ILogger<MongoPersonDomainEventHandler> _logger;
        private readonly ScopedConsumeContextProvider _scoped;

        public MongoPersonDomainEventHandler(ILogger<MongoPersonDomainEventHandler> logger,
            ScopedConsumeContextProvider scoped)
        {
            _logger = logger;
            _scoped = scoped;
        }

        public Task EntityAddedAsync(EntityAddedDomainEvent<MongoTenantPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var modified = domainEvent.Entity;
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);

            LogPersonAdded(_logger, modifiedJson, contextJson);
            LogCatchPhrase(_logger, domainEvent.EventContext.GetCustomContext<CatchPhraseModel>()?.CatchPhrase);

            if (_scoped.HasContext && _scoped.GetContext().TryGetMessage(out ConsumeContext<EntityAddedDomainEvent<EfPerson>> consumeContext))
            {
                LogCatchPhraseFromScope(_logger, consumeContext?.Message?.EventContext?.GetCustomContext<CatchPhraseModel>()?.CatchPhrase);
            }
            else
            {
                LogNoScopeContext(_logger);
            }

            return Task.CompletedTask;
        }

        public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<MongoTenantPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var original = domainEvent.Previous;
            var modified = domainEvent.Modified;
            var originalJson = JsonConvert.SerializeObject(original, Formatting.Indented);
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);

            LogPersonUpdated(_logger, originalJson, modifiedJson, contextJson);

            LogCatchPhrase(_logger, domainEvent.EventContext.GetCustomContext<CatchPhraseModel>()?.CatchPhrase);

            if (_scoped.HasContext && _scoped.GetContext().TryGetMessage(out ConsumeContext<EntityAddedDomainEvent<EfPerson>> consumeContext))
            {
                LogCatchPhraseFromScope(_logger, consumeContext?.Message?.EventContext?.GetCustomContext<CatchPhraseModel>()?.CatchPhrase);
            }
            else
            {
                LogNoScopeContext(_logger);
            }

            return Task.CompletedTask;
        }
    }
}
