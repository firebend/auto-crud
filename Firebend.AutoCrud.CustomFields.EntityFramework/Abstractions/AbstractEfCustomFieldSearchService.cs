using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractEfCustomFieldSearchService<TKey, TEntity, TCustomFieldsEntity> :
        BaseDisposable,
        ICustomFieldsSearchService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>
    {
        private readonly IEntityFrameworkQueryClient<TKey, TEntity> _queryClient;
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;
        private readonly IEntitySearchHandler<TKey, TEntity, CustomFieldsSearchRequest> _searchHandler;

        protected AbstractEfCustomFieldSearchService(IEntityFrameworkQueryClient<TKey, TEntity> queryClient,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator,
            IEntitySearchHandler<TKey, TEntity, CustomFieldsSearchRequest> searchHandler = null)
        {
            _queryClient = queryClient;
            _customFieldsStorageCreator = customFieldsStorageCreator;
            _searchHandler = searchHandler;
        }


        public async Task<EntityPagedResponse<CustomFieldsEntity<TKey>>> SearchAsync(CustomFieldsSearchRequest searchRequest,
            CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            var (query, context) = await _queryClient
                .GetQueryableAsync(true, cancellationToken)
                .ConfigureAwait(false);

            if (_searchHandler != null)
            {
                query = _searchHandler.HandleSearch(query, searchRequest)
                    ?? await _searchHandler.HandleSearchAsync(query, searchRequest);
            }

            var fieldsQuery = query.SelectMany(x => x.CustomFields);

            await using (context)
            {

                if (!string.IsNullOrWhiteSpace(searchRequest.Key))
                {
                    fieldsQuery = fieldsQuery.Where(x => x.Key == searchRequest.Key);
                }

                if (!string.IsNullOrWhiteSpace(searchRequest.Value))
                {
                    fieldsQuery = fieldsQuery.Where(x => x.Value.Contains(searchRequest.Value));
                }

                var count = await fieldsQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

                fieldsQuery = fieldsQuery.OrderBy(x => x.Key);

                if ((searchRequest.PageNumber ?? 0) > 0 && (searchRequest.PageSize ?? 0) > 0)
                {
                    fieldsQuery = fieldsQuery
                        .Skip((searchRequest.PageNumber!.Value - 1) * searchRequest.PageSize!.Value)
                        .Take(searchRequest.PageSize.Value);
                }

                var records = await fieldsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
                var data = records.Select(x => ((TCustomFieldsEntity)x).ToCustomFields()).ToList();
                return new EntityPagedResponse<CustomFieldsEntity<TKey>>
                {
                    Data = data,
                    CurrentPage = searchRequest.PageNumber,
                    TotalRecords = count,
                    CurrentPageSize = data.Count
                };
            }
        }

        protected override void DisposeManagedObjects()
        {
            _queryClient?.Dispose();
            _customFieldsStorageCreator.Dispose();
        }
    }
}
