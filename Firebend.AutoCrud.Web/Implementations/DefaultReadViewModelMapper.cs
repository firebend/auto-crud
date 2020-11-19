
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations
{
    public class DefaultReadViewModelMapper<TKey, TEntity> : IReadViewModelMapper<TKey, TEntity, TEntity>
        where TEntity : class, IEntity<TKey>
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
