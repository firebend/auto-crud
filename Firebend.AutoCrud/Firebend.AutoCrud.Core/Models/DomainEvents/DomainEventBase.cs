using System;

namespace Firebend.AutoCrud.Core.Models.DomainEvents
{
    public class DomainEventBase
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        
        public DomainEventContext EventContext { get; set; }
    }
}