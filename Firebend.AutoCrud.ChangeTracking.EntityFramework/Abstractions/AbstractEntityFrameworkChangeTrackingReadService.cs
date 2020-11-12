using System;
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
            base(contextProvider, null)
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
