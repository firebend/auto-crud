using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;

public class MongoChangeTrackingReadRepository<TEntityKey, TEntity> :
    AbstractEntitySearchService<ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>>,
    IChangeTrackingReadService<TEntityKey, TEntity>
    where TEntityKey : struct
    where TEntity : class, IEntity<TEntityKey>
{
    private readonly IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> _queryClient;
    private readonly IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> _searchHandler;

    public MongoChangeTrackingReadRepository(IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient,
        IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> searchHandler)
    {
        _queryClient = queryClient;
        _searchHandler = searchHandler;
    }

    public async Task<EntityPagedResponse<ChangeTrackingEntity<TEntityKey, TEntity>>> GetChangesByEntityId(
        ChangeTrackingSearchRequest<TEntityKey> searchRequest,
        CancellationToken cancellationToken)
    {
        if (searchRequest == null)
        {
            throw new ArgumentNullException(nameof(searchRequest));
        }

        Func<IQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>, Task<IQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>>> firstStageFilter = null;

        if (!string.IsNullOrWhiteSpace(searchRequest.Search))
        {
            firstStageFilter = async x =>
                (IQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>)_searchHandler.HandleSearch(x, searchRequest)
                    ?? (IQueryable<ChangeTrackingEntity<TEntityKey, TEntity>>)await _searchHandler
                        .HandleSearchAsync(x, searchRequest);
        }

        var query = await _queryClient.GetQueryableAsync(firstStageFilter, cancellationToken);

        query = query.Where(x => x.EntityId.Equals(searchRequest.EntityId));

        query = GetSearchExpressions(searchRequest).Aggregate(query, (current, expression) => current.Where(expression));

        if (searchRequest.OrderBy == null)
        {
            query = query.OrderByDescending(x => x.ModifiedDate);
        }

        var paged = await _queryClient.GetPagedResponseAsync(query, searchRequest, cancellationToken);

        return paged;
    }
}
