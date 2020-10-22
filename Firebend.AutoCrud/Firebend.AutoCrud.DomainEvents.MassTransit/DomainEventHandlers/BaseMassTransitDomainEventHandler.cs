using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.Interfaces;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers
{
    public abstract class BaseMassTransitDomainEventHandler<T> :
        IMassTransitDomainEventHandler<T>,
        IConsumer<T>
        where T : DomainEventBase
    {
        public abstract Task Consume(ConsumeContext<T> context);
    }
}