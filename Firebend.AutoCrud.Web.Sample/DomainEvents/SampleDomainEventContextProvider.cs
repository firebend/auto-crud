using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public class CatchPhraseModel
    {
        public string CatchPhrase { get; set; }
    }

    public class SampleDomainEventContextProvider : IDomainEventContextProvider
    {
        public DomainEventContext GetContext() => new DomainEventContext
        {
            Source = "My Sample",
            UserEmail = "sample@firebend.com",
            CustomContext = new CatchPhraseModel { CatchPhrase = "I Like Turtles" }
        };
    }
}
