using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers;

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
