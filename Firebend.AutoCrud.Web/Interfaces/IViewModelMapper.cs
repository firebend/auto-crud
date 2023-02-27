using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces
{
    public interface IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IApiVersion
        where TViewModel : class
    {
        Task<TEntity> FromAsync(TViewModel model, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TViewModel> model, CancellationToken cancellationToken = default);

        Task<TViewModel> ToAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<IEnumerable<TViewModel>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default);
    }

    public interface ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IApiVersion
        where TViewModel : class
    {

    }

    public interface IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IApiVersion
        where TViewModel : class
    {

    }

    public interface IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IApiVersion
        where TViewModel : class
    {

    }

    public interface ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>
        where TViewWrapper : IMultipleEntityViewModel<TView>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TVersion : class, IApiVersion
    {
        Task<TEntity> FromAsync(TViewWrapper wrapper, TView viewModel, CancellationToken cancellationToken = default);
    }
}
