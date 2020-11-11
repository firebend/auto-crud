using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultDomainEventContextProvider : IDomainEventContextProvider
    {
        public DomainEventContext GetContext() => new DomainEventContext();
    }
}
