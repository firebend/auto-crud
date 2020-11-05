using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntitySearchService<TKey, TEntity, TSearch> : IEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly IEntityDefaultOrderByProvider<TKey, TEntity> _orderByProvider;
        private readonly IEntityFrameworkQueryClient<TKey, TEntity> _searchClient;

        public EntityFrameworkEntitySearchService(IEntityFrameworkQueryClient<TKey, TEntity> searchClient,
            IEntityDefaultOrderByProvider<TKey, TEntity> orderByProvider)
        {
            _searchClient = searchClient;
            _orderByProvider = orderByProvider;
        }

        public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            var results = await PageAsync(request, cancellationToken);

            return results?.Data?.ToList();
        }

        public Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            return _searchClient.PageAsync(request?.Search,
                BuildSearchFilter(request),
                request?.PageNumber,
                request?.PageSize,
                request?.DoCount ?? false,
                GetOrderByGroups(request),
                cancellationToken
            );
        }

        protected virtual Expression<Func<TEntity, bool>> BuildSearchFilter(TSearch search)
        {
            return null;
        }

        protected virtual Expression<Func<TEntity, bool>> BuildSearchExpression(TSearch search)
        {

            var functions = new List<Expression<Func<TEntity, bool>>>();

            if (search is IActiveEntitySearchRequest activeEntitySearchRequest) {
                if (!activeEntitySearchRequest.IsDeleted.HasValue) {
                    var expression = (Expression<Func<IActiveEntity, bool>>)(x => x.IsDeleted == activeEntitySearchRequest.IsDeleted);
                    functions.Add(Expression.Lambda<Func<TEntity, bool>>(expression.Body, expression.Parameters));
                }
            }

            var customFilter = BuildSearchFilter(search);

            if (customFilter != null) {
                functions.Add(customFilter);
            }

            Expression<Func<TEntity, bool>> filters = null;

            for (int i = 0; i < functions.Count; i++) {
                if (i == 0) {
                    filters = functions[i];
                } else {
                    filters = filters.AndAlso(functions[i]);
                }
            }

            return filters;
        }

        private IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> GetOrderByGroups(TSearch search)
        {
            var orderByGroups = search?.OrderBy?.ToOrderByGroups<TEntity>()?.ToList();

            if (!(orderByGroups?.Any() ?? false))
            {
                var orderBy = _orderByProvider.OrderBy;

                if (orderBy != default && orderBy.func != null)
                {
                    orderByGroups = new List<(Expression<Func<TEntity, object>> order, bool ascending)>
                    {
                        orderBy
                    };
                }
            }

            return orderByGroups;
        }
    }
}