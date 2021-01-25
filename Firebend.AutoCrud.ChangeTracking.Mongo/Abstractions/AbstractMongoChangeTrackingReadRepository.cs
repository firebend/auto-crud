using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingReadRepository<TEntityKey, TEntity> :
        AbstractEntitySearchService<ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>>,
        IChangeTrackingReadService<TEntityKey, TEntity>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        private readonly IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> _queryClient;

        protected AbstractMongoChangeTrackingReadRepository(IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient)
        {
            _queryClient = queryClient;
        }

        public async Task<EntityPagedResponse<ChangeTrackingEntity<TEntityKey, TEntity>>> GetChangesByEntityId(
            ChangeTrackingSearchRequest<TEntityKey> searchRequest,
            CancellationToken cancellationToken = default)
        {
            if (searchRequest == null)
            {
                throw new ArgumentNullException(nameof(searchRequest));
            }

            FilterDefinition<ChangeTrackingEntity<TEntityKey, TEntity>> filterDefinition = null;

            if (!string.IsNullOrWhiteSpace(searchRequest.Search))
            {
                filterDefinition = Builders<ChangeTrackingEntity<TEntityKey, TEntity>>.Filter.Text(searchRequest.Search);
            }

            var query = await _queryClient.GetQueryableAsync(filterDefinition, cancellationToken);

            query = query.Where(x => x.EntityId.Equals(searchRequest.EntityId));

            var filter = GetSearchExpression(searchRequest);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (searchRequest.OrderBy == null)
            {
                query = query.OrderByDescending(x => x.ModifiedDate);
            }

            var paged = await _queryClient
                .GetPagedResponseAsync(query, searchRequest, cancellationToken)
                .ConfigureAwait(false);

            return paged;
        }
    }
}
