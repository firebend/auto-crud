using System;

namespace Firebend.AutoCrud.Core.Models.DomainEvents
{
    public class DomainEventBase
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();

        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;

        public DomainEventContext EventContext { get; set; }
    }
}
