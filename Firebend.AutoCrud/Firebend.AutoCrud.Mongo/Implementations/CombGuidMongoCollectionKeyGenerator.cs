using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class CombGuidMongoCollectionKeyGenerator<TEntity> : IMongoCollectionKeyGenerator<TEntity, Guid>
        where TEntity : IEntity<Guid>
    {
        public Task<Guid> GenerateKeyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((Guid)MongoIdGeneratorComb.Instance.GenerateId(Guid.NewGuid(), DateTime.UtcNow));
        }
    }
}