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
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities;

public abstract class MongoEntitySearchService<TKey, TEntity, TSearch> : AbstractEntitySearchService<TEntity, TSearch>,
    IEntitySearchService<TKey, TEntity, TSearch>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TSearch : IEntitySearchRequest
{
    private readonly IMongoReadClient<TKey, TEntity> _readClient;
    private readonly IEntitySearchHandler<TKey, TEntity, TSearch> _searchHandler;
    private readonly ISessionTransactionManager _transactionManager;

    protected MongoEntitySearchService(IMongoReadClient<TKey, TEntity> readClient,
        IEntitySearchHandler<TKey, TEntity, TSearch> searchHandler, ISessionTransactionManager transactionManager)
    {
        _readClient = readClient;
        _searchHandler = searchHandler;
        _transactionManager = transactionManager;
    }

    public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await SearchAsync(request, transaction, cancellationToken);
    }

    public async Task<List<TEntity>> SearchAsync(TSearch request, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var results = await PageAsync(request, entityTransaction, cancellationToken).ConfigureAwait(false);
        return results?.Data?.ToList();
    }

    public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await PageAsync(request, transaction, cancellationToken);
    }

    public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilter = null;

        if (_searchHandler != null)
        {
            firstStageFilter = x => (IMongoQueryable<TEntity>)_searchHandler.HandleSearch(x, request);
        }

        var query = await _readClient
            .GetQueryableAsync(firstStageFilter, entityTransaction, cancellationToken)
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
