using Firebend.AutoCrud.Core.Extensions;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages
{
    public class EntityDeletedDomainEvent<T> : DomainEventBase where T : class
    {
         public T Entity { get; set; }
    }
}