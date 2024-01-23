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

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions;

public abstract class AbstractEntityFrameworkChangeTrackingReadService<TEntityKey, TEntity> :
    AbstractEntitySearchService<ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>>,
    IChangeTrackingReadService<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    private readonly IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> _queryClient;
    private readonly IEntitySearchHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>> _searchHandler;

    protected AbstractEntityFrameworkChangeTrackingReadService(IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient,
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

        var (query, context) = await _queryClient
            .GetQueryableAsync(true, cancellationToken)
            .ConfigureAwait(false);

        await using var dbContext = context;

        query = query.Where(x => x.EntityId.Equals(searchRequest.EntityId));

        var filter = GetSearchExpression(searchRequest);

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (_searchHandler != null)
        {
            query = _searchHandler.HandleSearch(query, searchRequest)
                    ?? await _searchHandler.HandleSearchAsync(query, searchRequest);
        }

        var paged = await _queryClient
            .GetPagedResponseAsync(query, searchRequest, true, cancellationToken)
            .ConfigureAwait(false);

        return paged;
    }
}
