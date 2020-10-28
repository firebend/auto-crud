using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityAddedDomainEventHandler<TSubscriber, TEntity> :
        IConsumer<EntityAddedDomainEvent<TEntity>>
        where TEntity : class
        where TSubscriber : IEntityAddedDomainEventSubscriber<TEntity>
    {
        private readonly TSubscriber  _added;

        public MassTransitEntityAddedDomainEventHandler(TSubscriber added)
        {
            _added = added;
        }

        public Task Consume(ConsumeContext<EntityAddedDomainEvent<TEntity>> context)
            => _added.EntityAddedAsync(context.Message, context.CancellationToken);
    }
}