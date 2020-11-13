using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntityUpdateService<TKey, TEntity> : IEntityUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityFrameworkUpdateClient<TKey, TEntity> _updateClient;

        protected EntityFrameworkEntityUpdateService(IEntityFrameworkUpdateClient<TKey, TEntity> updateClient)
        {
            _updateClient = updateClient;
        }

        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => _updateClient.UpdateAsync(entity, cancellationToken);

        public virtual Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
            => _updateClient.UpdateAsync(key, jsonPatchDocument, cancellationToken);
    }
}
