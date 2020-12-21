using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.ChangeTracking.Models
{
    /// <summary>
    /// Encapsulates data when searching for change tracking events.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class ChangeTrackingSearchRequest<TKey> : EntitySearchRequest
    {
        /// <summary>
        /// Gets or sets a value indicating the id of the affected entity.
        /// </summary>
        public TKey EntityId { get; set; }
    }
}
