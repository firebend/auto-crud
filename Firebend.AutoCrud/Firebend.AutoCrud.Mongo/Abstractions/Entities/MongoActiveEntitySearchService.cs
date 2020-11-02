using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoActiveEntitySearchService<TKey, TEntity, TSearch> : MongoEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, IActiveEntity
        where TSearch : ActiveEntitySearchRequest
    
    {
        protected MongoActiveEntitySearchService(IMongoReadClient<TKey, TEntity> readClient,
            IEntityDefaultOrderByProvider<TKey, TEntity> orderByProvider) : base(readClient, orderByProvider)
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