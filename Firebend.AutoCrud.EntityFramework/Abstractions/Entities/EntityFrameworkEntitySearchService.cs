using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntitySearchService<TKey, TEntity, TSearch> : AbstractEntitySearchService<TEntity, TSearch>,
        IEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly IEntityFrameworkQueryClient<TKey, TEntity> _searchClient;
        private readonly IEntityFrameworkQueryableCustomizer<TKey, TEntity, TSearch> _customizer;

        public EntityFrameworkEntitySearchService(IEntityFrameworkQueryClient<TKey, TEntity> searchClient,
            IEntityFrameworkQueryableCustomizer<TKey, TEntity, TSearch> customizer)
        {
            _searchClient = searchClient;
            _customizer = customizer;
        }

        public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            request.DoCount = false;

            var results = await PageAsync(request, cancellationToken).ConfigureAwait(false);

            return results?.Data?.ToList();
        }

        public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken = default)
        {
            var query = await _searchClient
                .GetQueryableAsync(cancellationToken).ConfigureAwait(false);

            if (_customizer != null)
            {
                query = _customizer.Customize(query, request);
            }

            var expression = GetSearchExpression(request);

            if (expression != null)
            {
                query = query.Where(expression);
            }

            var paged = await _searchClient
                .GetPagedResponseAsync(query, request, cancellationToken)
                .ConfigureAwait(false);

            return paged;
        }

        protected override void DisposeManagedObjects() => _searchClient?.Dispose();
    }
}
