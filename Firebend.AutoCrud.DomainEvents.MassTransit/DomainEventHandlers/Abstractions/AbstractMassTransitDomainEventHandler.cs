using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers.Abstractions;

public abstract class AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler> : BaseDisposable, IConsumer<TDomainEvent>
    where TDomainEvent : DomainEventBase
    where TDomainEventHandler : class, IDomainEventSubscriber
{
    protected AbstractMassTransitDomainEventHandler(TDomainEventHandler handler)
    {
        Handler = handler;
    }

    public TDomainEventHandler Handler { get; private set; }

    public async Task Consume(ConsumeContext<TDomainEvent> context)
    {
        await ConsumeEvent(context);
        Handler.Dispose();
    }

    protected abstract Task ConsumeEvent(ConsumeContext<TDomainEvent> context);

    protected override void DisposeManagedObjects() => Handler.Dispose();

    protected override void DisposeUnmanagedObjectsAndAssignNull() => Handler = null;
}
