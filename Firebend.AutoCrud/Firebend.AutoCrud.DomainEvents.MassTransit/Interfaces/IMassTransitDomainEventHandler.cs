using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Interfaces
{
    public interface IMassTransitDomainEventHandler<T>
        where T:DomainEventBase
    {
        
    }
}