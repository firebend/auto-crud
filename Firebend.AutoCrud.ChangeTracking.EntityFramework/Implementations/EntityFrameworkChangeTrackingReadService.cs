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
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Implementations;

// A thin closure of the generic 3-arg version over the default row type, kept as its own named
// class since it predates custom row types and is part of the public API.
public class EntityFrameworkChangeTrackingReadService<TEntityKey, TEntity> :
    EntityFrameworkChangeTrackingReadService<TEntityKey, TEntity, ChangeTrackingEntity<TEntityKey, TEntity>>,
    IChangeTrackingReadService<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    public EntityFrameworkChangeTrackingReadService(IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient,
        IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> searchHandler)
        : base(queryClient, searchHandler)
    {
    }
}

/// <summary>
/// Encapsulates logic for reading change tracking events from a data store using Entity Framework,
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
public class EntityFrameworkChangeTrackingReadService<TEntityKey, TEntity, TChangeTrackingEntity> :
    AbstractEntitySearchService<TChangeTrackingEntity, ChangeTrackingSearchRequest<TEntityKey>>,
    IChangeTrackingReadService<TEntityKey, TEntity, TChangeTrackingEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
    where TChangeTrackingEntity : ChangeTrackingEntity<TEntityKey, TEntity>
{
    private readonly IEntityFrameworkQueryClient<Guid, TChangeTrackingEntity> _queryClient;
    private readonly IEntitySearchHandler<Guid, TChangeTrackingEntity, ChangeTrackingSearchRequest<TEntityKey>> _searchHandler;

    public EntityFrameworkChangeTrackingReadService(IEntityFrameworkQueryClient<Guid, TChangeTrackingEntity> queryClient,
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

        var (query, context) = await _queryClient
            .GetQueryableAsync(true, cancellationToken);

        await using (context)
        {
            query = query.Where(x => x.EntityId.Equals(searchRequest.EntityId));

            query = GetSearchExpressions(searchRequest).Aggregate(query, (current, expression) => current.Where(expression));

            if (_searchHandler != null)
            {
                query = _searchHandler.HandleSearch(query, searchRequest)
                        ?? await _searchHandler.HandleSearchAsync(query, searchRequest);
            }

            var paged = await _queryClient
                .GetPagedResponseAsync(query, searchRequest, true, cancellationToken);

            return paged;
        }
    }
}
