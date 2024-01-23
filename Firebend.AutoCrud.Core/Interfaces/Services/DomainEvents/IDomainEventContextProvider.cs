using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

public interface IDomainEventContextProvider
{
    DomainEventContext GetContext();
}
