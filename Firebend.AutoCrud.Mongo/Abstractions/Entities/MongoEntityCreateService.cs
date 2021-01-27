using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoEntityCreateService<TKey, TEntity> : BaseDisposable, IEntityCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoCreateClient<TKey, TEntity> _createClient;

        protected MongoEntityCreateService(IMongoCreateClient<TKey, TEntity> createClient)
        {
            _createClient = createClient;
        }

        public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default) => _createClient.CreateAsync(entity, cancellationToken);
    }
}
