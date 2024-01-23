namespace Firebend.AutoCrud.Core.Models.DomainEvents;

public class EntityDeletedDomainEvent<T> : DomainEventBase
    where T : class
{
    public T Entity { get; set; }
}
