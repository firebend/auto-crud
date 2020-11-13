using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoEntitySearchService<TKey, TEntity, TSearch> : AbstractEntitySearchService<TEntity, TSearch>,
        IEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly IEntityDefaultOrderByProvider<TKey, TEntity> _orderByProvider;
        private readonly IMongoReadClient<TKey, TEntity> _readClient;
        private readonly ISearchExpressionProvider<TKey, TEntity, TSearch> _searchExpressionProvider;

        protected MongoEntitySearchService(IMongoReadClient<TKey, TEntity> readClient,
            ISearchExpressionProvider<TKey, TEntity, TSearch> searchExpressionProvider = null,
            IEntityDefaultOrderByProvider<TKey, TEntity> orderByProvider = null)
        {
            _readClient = readClient;
            _searchExpressionProvider = searchExpressionProvider;
            _orderByProvider = orderByProvider;
        }

        public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            var results = await PageAsync(request, cancellationToken).ConfigureAwait(false);

            return results?.Data?.ToList();
        }

        public Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken = default)
            => _readClient.PageAsync(request?.Search,
                BuildSearchExpression(request),
                request?.PageNumber,
                request?.PageSize,
                request?.DoCount ?? false,
                GetOrderByGroups(request),
                cancellationToken
            );

        private IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> GetOrderByGroups(TSearch search)
        {
            var orderByGroups = search?.OrderBy?.ToOrderByGroups<TEntity>()?.ToList();

            if (orderByGroups != null && orderByGroups.Any())
            {
                return orderByGroups;
            }

            var orderBy = _orderByProvider?.OrderBy;

            if (orderBy.HasValue && orderBy.Value != default && orderBy.Value.func != null)
            {
                orderByGroups = new List<(Expression<Func<TEntity, object>> order, bool ascending)> { orderBy.Value };
            }

            return orderByGroups;
        }

        protected virtual Expression<Func<TEntity, bool>> BuildSearchFilter(TSearch search) => _searchExpressionProvider?.GetSearchExpression(search);

        protected virtual Expression<Func<TEntity, bool>> BuildSearchExpression(TSearch search) => GetSearchExpression(BuildSearchFilter(search), search);
    }
}
