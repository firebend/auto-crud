using Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Interfaces
{
    public interface IMassTransitDomainEventHandler<T>
        where T:DomainEventBase
    {
        
    }
}