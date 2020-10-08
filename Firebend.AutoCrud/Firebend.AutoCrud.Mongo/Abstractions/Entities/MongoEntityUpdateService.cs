using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public class MongoEntityUpdateService<TKey, TEntity> : IEntityUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoUpdateClient<TEntity, TKey> _updateClient;

        public MongoEntityUpdateService(IMongoUpdateClient<TEntity, TKey> updateClient)
        {
            _updateClient = updateClient;
        }

        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Allow creating entities through PUT to make it easier to set the guid in the client
            // when creating new entities. ( ACID2.0 )
            return _updateClient.UpsertAsync(entity, cancellationToken);
        }

        public Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            if (key.Equals(default))
            {
                throw new ArgumentException("Key is invalid", nameof(key));
            }

            return _updateClient.UpdateAsync(key, jsonPatchDocument, cancellationToken);
        }
    }
}