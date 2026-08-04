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

/// <summary>
/// Maps <typeparamref name="TChangeTrackingEntity"/> change tracking rows to <typeparamref name="TChangeTrackingViewModel"/>
/// view models, for a consumer using a custom row type and a custom DTO to expose its extra columns.
/// </summary>
public interface IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel, TChangeTrackingEntity, TChangeTrackingViewModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
    where TChangeTrackingViewModel : ChangeTrackingModel<TKey, TViewModel>, new()
{
    public Task<List<TChangeTrackingViewModel>> MapAsync(
        IEnumerable<TChangeTrackingEntity> changeTrackingEntities,
        CancellationToken cancellationToken);
}
