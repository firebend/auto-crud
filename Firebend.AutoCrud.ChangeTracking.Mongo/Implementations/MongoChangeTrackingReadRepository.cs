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

// A thin closure of the generic 3-arg version over the default row type, kept as its own named
// class since it predates custom row types and is part of the public API.
public class MongoChangeTrackingReadRepository<TEntityKey, TEntity> :
    MongoChangeTrackingReadRepository<TEntityKey, TEntity, ChangeTrackingEntity<TEntityKey, TEntity>>,
    IChangeTrackingReadService<TEntityKey, TEntity>
    where TEntityKey : struct
    where TEntity : class, IEntity<TEntityKey>
{
    public MongoChangeTrackingReadRepository(IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient,
        IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> searchHandler)
        : base(queryClient, searchHandler)
    {
    }
}

/// <summary>
/// Encapsulates logic for reading change tracking events from a data store using Mongo,
/// for a custom <typeparamref name="TChangeTrackingEntity"/> row type.
/// </summary>
/// <typeparam name="TEntityKey">
/// The type of key the entity uses.
/// </typeparam>
/// <typeparam name="TEntity">
/// The type of entity.
/// </typeparam>
/// <typeparam name="TChangeTrackingEntity">
/// The type of row persisted for each change. Must inherit <see cref="ChangeTrackingEntity{TKey,TEntity}"/>.
/// </typeparam>
public class MongoChangeTrackingReadRepository<TEntityKey, TEntity, TChangeTrackingEntity> :
    AbstractEntitySearchService<TChangeTrackingEntity, ChangeTrackingSearchRequest<TEntityKey>>,
    IChangeTrackingReadService<TEntityKey, TEntity, TChangeTrackingEntity>
    where TEntityKey : struct
    where TEntity : class, IEntity<TEntityKey>
    where TChangeTrackingEntity : ChangeTrackingEntity<TEntityKey, TEntity>
{
    private readonly IMongoReadClient<Guid, TChangeTrackingEntity> _queryClient;
    private readonly IEntitySearchHandler<Guid, TChangeTrackingEntity, ChangeTrackingSearchRequest<TEntityKey>> _searchHandler;

    public MongoChangeTrackingReadRepository(IMongoReadClient<Guid, TChangeTrackingEntity> queryClient,
        IEntitySearchHandler<Guid, TChangeTrackingEntity, ChangeTrackingSearchRequest<TEntityKey>> searchHandler)
    {
        _queryClient = queryClient;
        _searchHandler = searchHandler;
    }

    public async Task<EntityPagedResponse<TChangeTrackingEntity>> GetChangesByEntityId(
        ChangeTrackingSearchRequest<TEntityKey> searchRequest,
        CancellationToken cancellationToken)
    {
        if (searchRequest == null)
        {
            throw new ArgumentNullException(nameof(searchRequest));
        }

        Func<IQueryable<TChangeTrackingEntity>, Task<IQueryable<TChangeTrackingEntity>>> firstStageFilter = null;

        if (!string.IsNullOrWhiteSpace(searchRequest.Search))
        {
            firstStageFilter = async x =>
                (IQueryable<TChangeTrackingEntity>)_searchHandler.HandleSearch(x, searchRequest)
                    ?? (IQueryable<TChangeTrackingEntity>)await _searchHandler
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
