using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
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
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TKey : struct
        where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>
    {
        private readonly IEntityFrameworkQueryClient<Guid, TCustomFieldsEntity> _queryClient;
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;

        protected AbstractEfCustomFieldSearchService(IEntityFrameworkQueryClient<Guid, TCustomFieldsEntity> queryClient,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator)
        {
            _queryClient = queryClient;
            _customFieldsStorageCreator = customFieldsStorageCreator;
        }


        public async Task<EntityPagedResponse<CustomFieldsEntity<TKey>>> SearchAsync(string key,
            string value,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            var (query, context) = await _queryClient
                .GetQueryableAsync(true, cancellationToken)
                .ConfigureAwait(false);

            using (context)
            {

                if (!string.IsNullOrWhiteSpace(key))
                {
                    query = query.Where(x => x.Key == key);
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    query = query.Where(x => x.Value.Contains(value));
                }

                var count = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);

                query = query.OrderBy(x => x.Key);

                if ((pageNumber ?? 0) > 0 && (pageSize ?? 0) > 0)
                {
                    query = query
                        .Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
                }

                var records = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

                return new EntityPagedResponse<CustomFieldsEntity<TKey>>
                {
                    Data = records.Select(x => x.ToCustomFields()).ToList(),
                    CurrentPage = pageNumber,
                    TotalRecords = count,
                    CurrentPageSize = pageSize
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
