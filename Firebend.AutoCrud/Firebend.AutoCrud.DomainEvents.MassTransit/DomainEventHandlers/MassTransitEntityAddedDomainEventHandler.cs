using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityAddedDomainEventHandler<TEntity> : 
        BaseMassTransitDomainEventHandler<EntityAddedDomainEvent<TEntity>>
    {
        private readonly IEntityAddedDomainEventSubscriber<TEntity> _added;

        public MassTransitEntityAddedDomainEventHandler(IEntityAddedDomainEventSubscriber<TEntity> added)
        {
            _added = added;
        }

        public override Task Consume(ConsumeContext<EntityAddedDomainEvent<TEntity>> context)
            => _added.EntityAddedAsync(context.Message.Entity, context.CancellationToken);
    }
}