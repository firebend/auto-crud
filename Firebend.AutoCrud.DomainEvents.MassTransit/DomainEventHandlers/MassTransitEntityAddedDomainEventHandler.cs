using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers;

public class MassTransitEntityAddedDomainEventHandler<TDomainEventHandler, TEntity> :
    AbstractMassTransitDomainEventHandler<EntityAddedDomainEvent<TEntity>, TDomainEventHandler>
    where TEntity : class
    where TDomainEventHandler : class, IEntityAddedDomainEventSubscriber<TEntity>
{
    public MassTransitEntityAddedDomainEventHandler(TDomainEventHandler added) : base(added)
    {
    }

    protected override Task ConsumeEvent(ConsumeContext<EntityAddedDomainEvent<TEntity>> context)
        => Handler.EntityAddedAsync(context.Message, context.CancellationToken);
}
