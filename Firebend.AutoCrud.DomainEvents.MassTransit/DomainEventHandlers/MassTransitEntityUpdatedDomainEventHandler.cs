using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityUpdatedDomainEventHandler<TDomainEventHandler, TEntity> :
        AbstractMassTransitDomainEventHandler<EntityUpdatedDomainEvent<TEntity>, TDomainEventHandler>
        where TEntity : class
        where TDomainEventHandler : IEntityUpdatedDomainEventSubscriber<TEntity>
    {
        public MassTransitEntityUpdatedDomainEventHandler(TDomainEventHandler updated) : base(updated)
        {
        }

        protected override Task ConsumeEvent(ConsumeContext<EntityUpdatedDomainEvent<TEntity>> context)
            => Handler.EntityUpdatedAsync(context.Message, context.CancellationToken);
    }
}
