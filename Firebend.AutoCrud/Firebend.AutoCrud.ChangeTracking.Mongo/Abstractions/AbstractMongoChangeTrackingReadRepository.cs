using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingReadRepository<TEntityKey, TEntity> :
        MongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>,
        IChangeTrackingReadService<TEntityKey,TEntity>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        protected AbstractMongoChangeTrackingReadRepository(IMongoClient client,
            ILogger<MongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>> logger,
            IMongoEntityConfiguration<TEntityKey, TEntity> entityConfiguration) : 
            base(client,
                logger,
                new MongoChangeTrackingEntityConfiguration<TEntityKey, TEntity>(entityConfiguration))
        {
        }

        public Task<EntityPagedResponse<ChangeTrackingEntity<TEntityKey, TEntity>>> GetChangesByEntityId(
            ChangeTrackingSearchRequest<TEntityKey> searchRequest,
            CancellationToken cancellationToken = default)
        {
            if (searchRequest == null)
            {
                throw new ArgumentNullException(nameof(searchRequest));
            }
            
            return PageAsync(null,
                x => x.EntityId.Equals(searchRequest.EntityId),
                searchRequest.PageNumber.GetValueOrDefault(),
                searchRequest.PageSize.GetValueOrDefault(),
                cancellationToken: cancellationToken);
        }
    }
}