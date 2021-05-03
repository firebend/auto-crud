using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.ChangeTracking
{
    public class DefaultChangeTrackingOptionsProvider<TKey, TEntity> : IChangeTrackingOptionsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public DefaultChangeTrackingOptionsProvider(ChangeTrackingOptions options)
        {
            Options = options;
        }

        public ChangeTrackingOptions Options { get; }
    }
}
