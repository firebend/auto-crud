using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public abstract class AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler> : BaseDisposable, IConsumer<TDomainEvent>
        where TDomainEvent : DomainEventBase
        where TDomainEventHandler : class, IDomainEventSubscriber
    {
        private TDomainEventHandler _handler;

        protected AbstractMassTransitDomainEventHandler(TDomainEventHandler handler)
        {
            _handler = handler;
        }

        public TDomainEventHandler Handler => _handler;

        public async Task Consume(ConsumeContext<TDomainEvent> context)
        {
            await ConsumeEvent(context).ConfigureAwait(false);
            Handler.Dispose();
        }

        protected abstract Task ConsumeEvent(ConsumeContext<TDomainEvent> context);

        protected override void DisposeManagedObjects() => Handler.Dispose();

        protected override void DisposeUnmanagedObjectsAndAssignNull() => _handler = null;
    }
}
