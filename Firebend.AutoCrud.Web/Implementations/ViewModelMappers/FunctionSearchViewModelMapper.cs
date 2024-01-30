using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers;

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
