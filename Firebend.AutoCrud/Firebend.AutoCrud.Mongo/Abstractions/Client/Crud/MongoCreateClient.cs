using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoCreateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        protected MongoCreateClient(IMongoClient client,
            ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration) : base(client, logger, entityConfiguration)
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