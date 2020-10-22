using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.Interfaces;
using Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityUpdatedDomainEventHandler<TEntity> :
        BaseMassTransitDomainEventHandler<EntityUpdatedDomainEvent<TEntity>>
        where TEntity : class
    {
        private readonly IEntityUpdatedDomainEventSubscriber<TEntity> _updated;

        public MassTransitEntityUpdatedDomainEventHandler(IEntityUpdatedDomainEventSubscriber<TEntity> updated)
        {
            _updated = updated;
        }

        public override Task Consume(ConsumeContext<EntityUpdatedDomainEvent<TEntity>> context)
            => _updated.EntityUpdatedAsync(context.Message.Previous, context.Message.Modified, context.CancellationToken);
    }
}