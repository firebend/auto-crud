using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions
{
    public abstract class AbstractEntityFrameworkChangeTrackingReadService<TEntityKey, TEntity> :
        EntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>,
        IChangeTrackingReadService<TEntityKey, TEntity>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
    {
        public AbstractEntityFrameworkChangeTrackingReadService(IChangeTrackingDbContextProvider<TEntityKey, TEntity> contextProvider) :
            base(contextProvider)
        {
        }

        public async Task<EntityPagedResponse<ChangeTrackingEntity<TEntityKey, TEntity>>> GetChangesByEntityId(
            ChangeTrackingSearchRequest<TEntityKey> searchRequest,
            CancellationToken cancellationToken = default)
        {
            if (searchRequest == null)
            {
                throw new ArgumentNullException(nameof(searchRequest));
            }

            var query = await GetQueryableAsync(cancellationToken);
            query = query.Where(x => x.EntityId.Equals(searchRequest.EntityId));

            if (searchRequest.OrderBy == null)
            {
                query = query.OrderByDescending(x => x.Modified);
            }

            var paged = await GetPagedResponseAsync(query, searchRequest, cancellationToken).ConfigureAwait(false);

            return paged;
        }
    }
}
