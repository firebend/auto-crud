using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoEntitySoftDeleteService<TKey, TEntity> : BaseDisposable, IEntityDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, IActiveEntity
    {
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;

        protected MongoEntitySoftDeleteService(IEntityUpdateService<TKey, TEntity> updateService)
        {
            _updateService = updateService;
        }

        protected virtual Task<TEntity> DeleteInternalAsync(TKey key,
           IEntityTransaction entityTransaction = null,
           CancellationToken cancellationToken = default)
        {
            var patch = new JsonPatchDocument<TEntity>();

            patch.Add(x => x.IsDeleted, true);

            return entityTransaction != null ?
                _updateService.PatchAsync(key, patch, entityTransaction, cancellationToken) :
                _updateService.PatchAsync(key, patch, cancellationToken);
        }

        public Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
            => DeleteInternalAsync(key, null, cancellationToken);

        public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
            => DeleteInternalAsync(key, entityTransaction, cancellationToken);
    }
}
