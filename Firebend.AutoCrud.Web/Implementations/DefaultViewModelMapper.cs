namespace Firebend.AutoCrud.Web.Implementations
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interfaces.Models;
    using Interfaces;

    public class DefaultViewModelMapper<TKey, TEntity> : IViewModelMapper<TKey, TEntity, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
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
