using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
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
        private readonly IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> _searchHandler;

        protected AbstractMongoChangeTrackingReadRepository(IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient,
            IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> searchHandler)
        {
            _queryClient = queryClient;
            _searchHandler = searchHandler;
        }

        public async Task<EntityPagedResponse<ChangeTrackingEntity<TEntityKey, TEntity>>> GetChangesByEntityId(
            ChangeTrackingSearchRequest<TEntityKey> searchRequest,
            CancellationToken cancellationToken = default)
        {
            if (searchRequest == null)
            {
                throw new ArgumentNullException(nameof(searchRequest));
            }

            Func<IMongoQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>, IMongoQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>> firstStageFilter = null;

            if (_searchHandler != null && !string.IsNullOrWhiteSpace(searchRequest?.Search))
            {
                firstStageFilter = x => (IMongoQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>)_searchHandler.HandleSearch(x, searchRequest);
            }

            var query = await _queryClient.GetQueryableAsync(firstStageFilter, cancellationToken);

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
