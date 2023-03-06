using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class DefaultReadViewModelMapper<TKey, TEntity, TVersion> : IReadViewModelMapper<TKey, TEntity, TVersion, TEntity>
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TKey : struct
    {
        public Task<TEntity> FromAsync(TEntity model, CancellationToken cancellationToken = default)
            => Task.FromResult(model);

        public Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TEntity> model, CancellationToken cancellationToken = default)
            => Task.FromResult(model);

        public Task<TEntity> ToAsync(TEntity entity, CancellationToken cancellationToken = default)
            => Task.FromResult(entity);

        public Task<IEnumerable<TEntity>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default)
            => Task.FromResult(entity);
    }
}
