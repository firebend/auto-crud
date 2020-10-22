using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public class SampleDomainEventContextProvider : IDomainEventContextProvider
    {
        public DomainEventContext GetContext() => new DomainEventContext
        {
            Source = "My Sample",
            UserName = "sample@firebend.com"
        };
    }
}