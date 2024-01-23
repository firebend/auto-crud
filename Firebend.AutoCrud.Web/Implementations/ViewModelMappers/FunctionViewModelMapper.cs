using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers;

public class FunctionViewModelMapper<TKey, TEntity, TVersion, TViewModel> : ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel>,
    IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel>,
    IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
{
    private static Func<TEntity, TViewModel> _to;
    private static Func<TViewModel, TEntity> _from;

    public FunctionViewModelMapper()
    {

    }

    public FunctionViewModelMapper(Func<TEntity, TViewModel> to)
    {
        _to = to;
    }

    public FunctionViewModelMapper(Func<TViewModel, TEntity> from)
    {
        _from = from;
    }

    public FunctionViewModelMapper(Func<TViewModel, TEntity> from, Func<TEntity, TViewModel> to)
    {
        _from = from;
        _to = to;
    }

    private static TEntity ToEntity(TViewModel model)
        => _from?.Invoke(model);

    private static TViewModel ToViewModel(TEntity entity)
        => _to?.Invoke(entity);

    public Task<TEntity> FromAsync(TViewModel model, CancellationToken cancellationToken = default)
        => Task.FromResult(ToEntity(model));

    public Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TViewModel> model, CancellationToken cancellationToken = default)
        => _from == null ? Task.FromResult((IEnumerable<TEntity>)null) : Task.FromResult(model.Select(ToEntity));

    public Task<TViewModel> ToAsync(TEntity entity, CancellationToken cancellationToken = default)
        => Task.FromResult(ToViewModel(entity));

    public Task<IEnumerable<TViewModel>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default)
        => _to == null ? Task.FromResult((IEnumerable<TViewModel>)null) : Task.FromResult(entity.Select(ToViewModel));
}

public class FunctionSearchViewModelMapper<TKey, TEntity, TVersion, TViewModel, TSearchModel>
    : ISearchViewModelMapper<TKey, TEntity, TVersion, TViewModel, TSearchModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TSearchModel : class
{
    private static Func<TViewModel, TSearchModel> _from;

    public FunctionSearchViewModelMapper()
    {

    }

    public FunctionSearchViewModelMapper(Func<TViewModel, TSearchModel> from)
    {
        _from = from;
    }

    public Task<TSearchModel> FromAsync(TViewModel model, CancellationToken cancellationToken = default)
        => Task.FromResult(_from?.Invoke(model));
}

public class IdentitySearchViewModelMapper<TKey, TEntity, TVersion, TSearchModel>
    : ISearchViewModelMapper<TKey, TEntity, TVersion, TSearchModel, TSearchModel>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TSearchModel : class
{
    public Task<TSearchModel> FromAsync(TSearchModel model, CancellationToken cancellationToken = default)
        => Task.FromResult(model);
}
