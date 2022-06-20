using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public abstract class AbstractMongoCustomFieldsSearchService<TKey, TEntity> : BaseDisposable,
    ICustomFieldsSearchService<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    private readonly IMongoReadClient<TKey, TEntity> _readClient;
    private readonly IEntitySearchHandler<TKey, TEntity, CustomFieldsSearchRequest> _searchHandler;

    protected AbstractMongoCustomFieldsSearchService(IMongoReadClient<TKey, TEntity> readClient,
        IEntitySearchHandler<TKey, TEntity, CustomFieldsSearchRequest> searchHandler = null)
    {
        _readClient = readClient;
        _searchHandler = searchHandler;
    }

    public async Task<EntityPagedResponse<CustomFieldsEntity<TKey>>> SearchAsync(
        CustomFieldsSearchRequest searchRequest,
        CancellationToken cancellationToken = default)
    {
        Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilter = null;

        if (_searchHandler != null)
        {
            firstStageFilter = x => (IMongoQueryable<TEntity>)_searchHandler.HandleSearch(x, searchRequest);
        }

        var query = await _readClient
            .GetQueryableAsync(firstStageFilter, cancellationToken)
            .ConfigureAwait(false);

        var fieldsQuery = query.SelectMany(x => x.CustomFields);

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

        return new EntityPagedResponse<CustomFieldsEntity<TKey>>
        {
            Data = records,
            CurrentPage = searchRequest.PageNumber,
            TotalRecords = count,
            CurrentPageSize = records.Count
        };
    }
}
