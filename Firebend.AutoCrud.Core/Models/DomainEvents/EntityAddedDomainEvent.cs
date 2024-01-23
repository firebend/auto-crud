namespace Firebend.AutoCrud.Core.Models.DomainEvents;

public class EntityAddedDomainEvent<T> : DomainEventBase
    where T : class
{
    public T Entity { get; set; }
}
