using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkActiveEntitySearchService<TKey, TEntity, TSearch> :
        EntityFrameworkEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, IActiveEntity
        where TSearch : ActiveEntitySearchRequest
    {
        protected EntityFrameworkActiveEntitySearchService(IEntityFrameworkQueryClient<TKey, TEntity> searchClient,
            IEntityDefaultOrderByProvider<TKey, TEntity> orderByProvider) : base(searchClient, orderByProvider)
        {
        }

        protected virtual Expression<Func<TEntity, bool>> AppendSearchFilter(TSearch search)
        {
            return null;
        }

        protected override Expression<Func<TEntity, bool>> BuildSearchFilter(TSearch search)
        {

            Expression<Func<TEntity, bool>> activeFilter = null;
            
            if ((search?.IsDeleted).HasValue)
            {
                activeFilter = x => x.IsDeleted == search.IsDeleted.Value;
            }

            var otherFilters = AppendSearchFilter(search);

            if (otherFilters != null && activeFilter != null)
            {
                return otherFilters.AndAlso(activeFilter);
            }

            if (otherFilters != null)
            {
                return otherFilters;
            }

            return activeFilter;
        }
    }
}