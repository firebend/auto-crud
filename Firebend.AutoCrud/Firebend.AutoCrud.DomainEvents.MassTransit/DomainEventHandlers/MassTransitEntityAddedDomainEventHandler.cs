using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public class MassTransitEntityAddedDomainEventHandler<TDomainEventHandler, TEntity> :
        AbstractMassTransitDomainEventHandler<EntityAddedDomainEvent<TEntity>, TDomainEventHandler>
        where TEntity : class
        where TDomainEventHandler : IEntityAddedDomainEventSubscriber<TEntity>
    {
        public MassTransitEntityAddedDomainEventHandler(TDomainEventHandler added) : base(added)
        {
        }

        public override Task Consume(ConsumeContext<EntityAddedDomainEvent<TEntity>> context)
            => Handler.EntityAddedAsync(context.Message, context.CancellationToken);
    }
}
