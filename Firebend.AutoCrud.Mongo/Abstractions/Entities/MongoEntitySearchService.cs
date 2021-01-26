using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities
{
    public abstract class MongoEntitySearchService<TKey, TEntity, TSearch> : AbstractEntitySearchService<TEntity, TSearch>,
        IEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly IMongoReadClient<TKey, TEntity> _readClient;
        private readonly IEntitySearchHandler<TKey, TEntity, TSearch> _searchHandler;

        protected MongoEntitySearchService(IMongoReadClient<TKey, TEntity> readClient,
            IEntitySearchHandler<TKey, TEntity, TSearch> searchHandler)
        {
            _readClient = readClient;
            _searchHandler = searchHandler;
        }

        public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            var results = await PageAsync(request, cancellationToken).ConfigureAwait(false);
            return results?.Data?.ToList();
        }

        public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilter = null;

            if (_searchHandler != null && !string.IsNullOrWhiteSpace(request?.Search))
            {
                firstStageFilter = x => (IMongoQueryable<TEntity>)_searchHandler.HandleSearch(x, request);
            }

            var query = await _readClient
                .GetQueryableAsync(firstStageFilter, cancellationToken)
                .ConfigureAwait(false);

            var expression = GetSearchExpression(request);

            if (expression != null)
            {
                query = query.Where(expression);
            }

            var paged = await _readClient
                .GetPagedResponseAsync(query, request, cancellationToken)
                .ConfigureAwait(false);

            return paged;
        }
    }
}
