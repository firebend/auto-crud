using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    where TEntity : IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TViewModel : class
{
    public Task<TEntity> FromAsync(TViewModel model, CancellationToken cancellationToken);

    public Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TViewModel> model, CancellationToken cancellationToken);

    public Task<TViewModel> ToAsync(TEntity entity, CancellationToken cancellationToken);

    public Task<IEnumerable<TViewModel>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken);
}
