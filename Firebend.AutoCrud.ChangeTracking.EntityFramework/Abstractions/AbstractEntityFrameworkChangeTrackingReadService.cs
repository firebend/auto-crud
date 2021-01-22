using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions
{
    public abstract class AbstractEntityFrameworkChangeTrackingReadService<TEntityKey, TEntity> :
        AbstractEntitySearchService<ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingSearchRequest<TEntityKey>>,
        IChangeTrackingReadService<TEntityKey, TEntity>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
    {
        private readonly IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> _queryClient;

        public AbstractEntityFrameworkChangeTrackingReadService(IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> queryClient)
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

            var query = await _queryClient.GetQueryableAsync(cancellationToken);

            query = query.Where(x => x.EntityId.Equals(searchRequest.EntityId));

            if (!string.IsNullOrWhiteSpace(searchRequest.Search))
            {
                if (!searchRequest.Search.Contains("%"))
                {
                    searchRequest.Search = $"%{searchRequest.Search}%";
                }

                query = query.Where(x =>
                    EF.Functions.JsonContainsAny(x.Changes, searchRequest.Search) ||
                    EF.Functions.JsonContainsAny(x.Entity, searchRequest.Search));
            }

            var filter = GetSearchExpression(searchRequest);

            query = query.Where(filter);

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
