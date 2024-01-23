namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;

public enum AutoCrudMassTransitQueueMode
{
    Unknown = 1,
    OneQueue = 2,
    QueuePerEntity = 3,
    QueuePerAction = 4,
    QueuePerEntityAction = 5
}
