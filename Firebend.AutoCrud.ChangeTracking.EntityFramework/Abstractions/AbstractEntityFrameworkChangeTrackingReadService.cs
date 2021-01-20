using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
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
            base(contextProvider, null, null)
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
                orderBys: GetOrderByGroups(searchRequest),
                cancellationToken: cancellationToken);
        }

        private static IEnumerable<(Expression<Func<ChangeTrackingEntity<TEntityKey, TEntity>, object>> order, bool ascending)> GetOrderByGroups(EntitySearchRequest search)
        {
            var orderByGroups = search?.OrderBy?.ToOrderByGroups<ChangeTrackingEntity<TEntityKey, TEntity>>()?.ToList();

            if (orderByGroups.HasValues())
            {
                return orderByGroups;
            }

            return new (Expression<Func<ChangeTrackingEntity<TEntityKey, TEntity>, object>> order, bool @ascending)[]
            {
                (x => x.Modified, false)
            };
        }
    }
}
