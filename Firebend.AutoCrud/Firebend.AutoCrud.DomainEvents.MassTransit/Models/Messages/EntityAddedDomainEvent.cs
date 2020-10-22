namespace Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages
{
    public class EntityAddedDomainEvent<T> : DomainEventBase
    {
        public T Entity { get; set; }
    }
}