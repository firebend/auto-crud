using System;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages
{
    public class DomainEventBase
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
    }
}