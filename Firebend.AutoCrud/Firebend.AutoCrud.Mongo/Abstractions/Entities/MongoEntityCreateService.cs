using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using SharpCompress.Common.Tar;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public class MongoEntityCreateService<TEntity, TKey> : IEntityCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoCreateClient<TEntity, TKey> _createClient;

        public MongoEntityCreateService(IMongoCreateClient<TEntity, TKey> createClient)
        {
            _createClient = createClient;
        }

        public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _createClient.CreateAsync(entity, cancellationToken);
        }
    }
}