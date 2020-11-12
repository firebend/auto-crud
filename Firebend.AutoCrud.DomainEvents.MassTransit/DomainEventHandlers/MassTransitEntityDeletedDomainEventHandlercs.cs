using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityDeletedDomainEventHandler<TDomainEventHandler, TEntity> :
        AbstractMassTransitDomainEventHandler<EntityDeletedDomainEvent<TEntity>, TDomainEventHandler>
        where TEntity : class
        where TDomainEventHandler : IEntityDeletedDomainEventSubscriber<TEntity>
    {
        public MassTransitEntityDeletedDomainEventHandler(TDomainEventHandler deleted) : base(deleted)
        {
        }

        public override Task Consume(ConsumeContext<EntityDeletedDomainEvent<TEntity>> context)
            => Handler.EntityDeletedAsync(context.Message, context.CancellationToken);
    }
}
