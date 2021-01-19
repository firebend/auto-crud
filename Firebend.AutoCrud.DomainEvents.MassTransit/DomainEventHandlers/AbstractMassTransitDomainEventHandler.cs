using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public abstract class AbstractMassTransitDomainEventHandler<TDomainEvent, TDomainEventHandler> : IConsumer<TDomainEvent>
        where TDomainEvent : DomainEventBase
        where TDomainEventHandler : IDomainEventSubscriber
    {
        protected AbstractMassTransitDomainEventHandler(TDomainEventHandler handler)
        {
            Handler = handler;
        }

        public TDomainEventHandler Handler { get; }

        public async Task Consume(ConsumeContext<TDomainEvent> context)
        {
            await ConsumeEvent(context).ConfigureAwait(false);
            Handler.Dispose();
        }

        protected abstract Task ConsumeEvent(ConsumeContext<TDomainEvent> context);
    }
}
