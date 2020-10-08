using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public class MongoEntityReadService<TKey, TEntity> : IEntityReadService<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private readonly IMongoReadClient<TEntity, TKey> _readClient;

        public MongoEntityReadService(IMongoReadClient<TEntity, TKey> readClient)
        {
            _readClient = readClient;
        }

        public Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return _readClient.SingleOrDefaultAsync(x => x.Id.Equals(key), cancellationToken);
        }

        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _readClient.GetAllAsync(cancellationToken);
        }
    }
}