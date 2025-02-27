using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.ChangeTracking.Web.Interfaces;

public interface IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
{
    public Task<List<ChangeTrackingModel<TKey, TViewModel>>> MapAsync(
        IEnumerable<ChangeTrackingEntity<TKey, TEntity>> changeTrackingEntities,
        CancellationToken cancellationToken);
}
