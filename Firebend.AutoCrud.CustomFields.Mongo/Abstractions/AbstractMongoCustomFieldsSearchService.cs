using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions
{
    public class AbstractMongoCustomFieldsSearchService<TKey, TEntity> : BaseDisposable, ICustomFieldsSearchService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IMongoReadClient<TKey, TEntity> _readClient;

        public AbstractMongoCustomFieldsSearchService(IMongoReadClient<TKey, TEntity> readClient)
        {
            _readClient = readClient;
        }

        public async Task<EntityPagedResponse<CustomFieldsEntity<TKey>>> SearchAsync(string key,
            string value,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = await _readClient
                .GetQueryableAsync(null, cancellationToken)
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
    }
}
