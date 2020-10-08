using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoCreateClient<TEntity, TKey> : MongoClientBaseEntity<TEntity, TKey>, IMongoCreateClient<TEntity, TKey>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        protected MongoCreateClient(IMongoClient client,
            ILogger<MongoCreateClient<TEntity, TKey>> logger,
            IMongoEntityConfiguration entityConfiguration) : base(client, logger, entityConfiguration)
        {
        }

        public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();
            await RetryErrorAsync(() => mongoCollection.InsertOneAsync(entity, null, cancellationToken));
            //todo: domain events?
            //await PublishAddAsync(entity, cancellationToken);
            return entity;
        }
    }
}