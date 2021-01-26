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
        private readonly IEntityQueryCustomizer<TKey, TEntity, TSearch> _queryCustomizer;

        protected MongoEntitySearchService(IMongoReadClient<TKey, TEntity> readClient,
            IEntityQueryCustomizer<TKey, TEntity, TSearch> queryCustomizer)
        {
            _readClient = readClient;
            _queryCustomizer = queryCustomizer;
        }

        public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            var results = await PageAsync(request, cancellationToken).ConfigureAwait(false);
            return results?.Data?.ToList();
        }

        public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            FilterDefinition<TEntity> searchExpression = null;

            if (!string.IsNullOrWhiteSpace(request?.Search))
            {
                searchExpression = Builders<TEntity>.Filter.Text(request.Search);
            }

            var query = await _readClient
                .GetQueryableAsync(searchExpression, cancellationToken).ConfigureAwait(false);

            if (_queryCustomizer != null)
            {
                query = _queryCustomizer.Customize(query, request);
            }

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
