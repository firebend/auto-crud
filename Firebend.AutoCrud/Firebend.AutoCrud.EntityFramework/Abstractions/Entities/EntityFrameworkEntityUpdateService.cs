using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public class EntityFrameworkEntityUpdateService<TKey, TEntity> : IEntityUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityFrameworkUpdateClient<TKey, TEntity> _updateClient;

        public EntityFrameworkEntityUpdateService(IEntityFrameworkUpdateClient<TKey, TEntity> updateClient)
        {
            _updateClient = updateClient;
        }

        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _updateClient.UpdateAsync(entity, cancellationToken);
        }

        public Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            return _updateClient.UpdateAsync(key, jsonPatchDocument, cancellationToken);
        }
    }
}