using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>
    where TViewWrapper : IMultipleEntityViewModel<TView>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TVersion : class, IAutoCrudApiVersion
{
    public Task<TEntity> FromAsync(TViewWrapper wrapper, TView viewModel, CancellationToken cancellationToken);
}
