using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractEfCustomFieldSearchService<TKey, TEntity> :
        BaseDisposable,
        ICustomFieldsSearchService<TKey, TEntity>
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TKey : struct
    {
        private readonly IEntityFrameworkQueryClient<TKey, TEntity> _queryClient;

        public AbstractEfCustomFieldSearchService(IEntityFrameworkQueryClient<TKey, TEntity> queryClient)
        {
            _queryClient = queryClient;
        }

        public async Task<EntityPagedResponse<CustomFieldsEntity<TKey>>> SearchAsync(string key,
            string value,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = await _queryClient
                .GetQueryableAsync(true,  cancellationToken)
                .ConfigureAwait(false);

            var fieldsQuery = query.SelectMany(x => x.CustomFields);

            if (!string.IsNullOrWhiteSpace(key))
            {
                fieldsQuery = fieldsQuery.Where(x => x.Key == key);
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                fieldsQuery = fieldsQuery.Where(x => x.Value.Contains(value));
            }

            var count = await fieldsQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

            fieldsQuery = fieldsQuery.OrderBy(x => x.Key);

            if ((pageNumber ?? 0) > 0 && (pageSize ?? 0) > 0)
            {
                fieldsQuery = fieldsQuery
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var records = await fieldsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            return new EntityPagedResponse<CustomFieldsEntity<TKey>>
            {
                Data = records, CurrentPage = pageNumber, TotalRecords = count, CurrentPageSize = pageSize
            };
        }

        protected override void DisposeManagedObjects() => _queryClient?.Dispose();
    }
}
