using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.ChangeTracking.Models
{
    public class ChangeTrackingSearchRequest<TKey> : EntitySearchRequest
    {
        public TKey EntityId { get; set; }
    }
}