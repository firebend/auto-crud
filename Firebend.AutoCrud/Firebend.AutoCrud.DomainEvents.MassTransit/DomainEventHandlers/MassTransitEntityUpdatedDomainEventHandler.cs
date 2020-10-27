using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityUpdatedDomainEventHandler<TSubscriber, TEntity> :
        IConsumer<EntityUpdatedDomainEvent<TEntity>>
        where TSubscriber : IEntityUpdatedDomainEventSubscriber<TEntity>
        where TEntity : class
    {
        private readonly TSubscriber _updated;

        public MassTransitEntityUpdatedDomainEventHandler(TSubscriber updated)
        {
            _updated = updated;
        }

        public Task Consume(ConsumeContext<EntityUpdatedDomainEvent<TEntity>> context)
            => _updated.EntityUpdatedAsync(context.Message, context.CancellationToken);
    }
}