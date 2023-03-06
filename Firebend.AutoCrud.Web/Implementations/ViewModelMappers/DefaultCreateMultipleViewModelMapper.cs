using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Models;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class DefaultCreateMultipleViewModelMapper<TKey, TEntity, TVersion> : ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, MultipleEntityViewModel<TEntity>, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IAutoCrudApiVersion
    {
        public Task<TEntity> FromAsync(MultipleEntityViewModel<TEntity> wrapper, TEntity viewModel, CancellationToken cancellationToken = default)
            => Task.FromResult(viewModel);
    }
}
