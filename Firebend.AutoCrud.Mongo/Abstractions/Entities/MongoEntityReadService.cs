using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoEntityReadService<TKey, TEntity> : BaseDisposable, IEntityReadService<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private readonly IMongoReadClient<TKey, TEntity> _readClient;

        protected MongoEntityReadService(IMongoReadClient<TKey, TEntity> readClient)
        {
            _readClient = readClient;
        }

        public Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default) =>
            _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), cancellationToken);

        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
            _readClient.GetAllAsync(null, cancellationToken);
    }
}
