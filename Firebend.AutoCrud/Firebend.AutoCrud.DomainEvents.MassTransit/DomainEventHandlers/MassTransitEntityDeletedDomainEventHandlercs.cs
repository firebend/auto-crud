using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityDeletedDomainEventHandler<TSubscriber, TEntity> :
        IConsumer<EntityDeletedDomainEvent<TEntity>>
        where TEntity : class
        where TSubscriber : IEntityDeletedDomainEventSubscriber<TEntity>
    {
        private readonly TSubscriber _deleted;

        public MassTransitEntityDeletedDomainEventHandler(TSubscriber deleted)
        {
            _deleted = deleted;
        }

        public Task Consume(ConsumeContext<EntityDeletedDomainEvent<TEntity>> context)
            => _deleted.EntityDeletedAsync(context.Message, context.CancellationToken);
    }
}