using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Models
{
    public class ChangeTrackingOptions
    {
        /// <summary>
        /// Specify true if you wish to persist <see cref="DomainEventContext.CustomContext"/> to the change tracking data store.
        /// </summary>
        public bool PersistCustomContext { get; set; }
    }
}
