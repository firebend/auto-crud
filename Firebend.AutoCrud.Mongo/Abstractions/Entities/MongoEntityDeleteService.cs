using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoEntityDeleteService<TKey, TEntity> : BaseDisposable, IEntityDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoDeleteClient<TKey, TEntity> _deleteClient;

        protected MongoEntityDeleteService(IMongoDeleteClient<TKey, TEntity> deleteClient)
        {
            _deleteClient = deleteClient;
        }

        public Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key.Equals(default))
            {
                throw new ArgumentException("Key is invalid", nameof(key));
            }

            return _deleteClient.DeleteAsync(x => x.Id.Equals(key), cancellationToken);
        }

        public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
        {
            if (key.Equals(default))
            {
                throw new ArgumentException("Key is invalid", nameof(key));
            }

            return _deleteClient.DeleteAsync(x => x.Id.Equals(key), entityTransaction, cancellationToken);
        }
    }
}
