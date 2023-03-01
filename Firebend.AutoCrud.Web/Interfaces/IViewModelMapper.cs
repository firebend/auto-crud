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
        where TVersion : class, IAutoCrudApiVersion
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
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {

    }

    public interface ISearchViewModelMapper<TKey, TEntity, TVersion, in TViewModel, TSearchModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {
        Task<TSearchModel> FromAsync(TViewModel model, CancellationToken cancellationToken = default);
    }

    public interface IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {

    }

    public interface IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {

    }

    public interface ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>
        where TViewWrapper : IMultipleEntityViewModel<TView>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        Task<TEntity> FromAsync(TViewWrapper wrapper, TView viewModel, CancellationToken cancellationToken = default);
    }
}
