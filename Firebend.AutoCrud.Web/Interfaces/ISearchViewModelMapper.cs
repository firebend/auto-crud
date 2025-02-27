using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface ISearchViewModelMapper<TKey, TEntity, TVersion, in TViewModel, TSearchModel>
    where TEntity : IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TViewModel : class
{
    public Task<TSearchModel> FromAsync(TViewModel model, CancellationToken cancellationToken);
}
