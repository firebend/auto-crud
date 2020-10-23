using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityDeletedDomainEventHandler<TEntity> :
        BaseMassTransitDomainEventHandler<EntityDeletedDomainEvent<TEntity>>
        where TEntity : class
    {
        private readonly IEntityDeletedDomainEventSubscriber<TEntity> _deleted;

        public MassTransitEntityDeletedDomainEventHandler(IEntityDeletedDomainEventSubscriber<TEntity> deleted)
        {
            _deleted = deleted;
        }

        public override Task Consume(ConsumeContext<EntityDeletedDomainEvent<TEntity>> context)
            => _deleted.EntityDeletedAsync(context.Message, context.CancellationToken);
    }
}