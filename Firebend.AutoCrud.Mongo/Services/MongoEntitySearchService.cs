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

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntitySearchService<TKey, TEntity, TSearch> : AbstractEntitySearchService<TEntity, TSearch>,
    IEntitySearchService<TKey, TEntity, TSearch>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TSearch : IEntitySearchRequest
{
    private readonly IMongoReadClient<TKey, TEntity> _readClient;
    private readonly IEntitySearchHandler<TKey, TEntity, TSearch> _searchHandler;
    private readonly ISessionTransactionManager _transactionManager;

    public MongoEntitySearchService(IMongoReadClient<TKey, TEntity> readClient,
        IEntitySearchHandler<TKey, TEntity, TSearch> searchHandler, ISessionTransactionManager transactionManager)
    {
        _readClient = readClient;
        _searchHandler = searchHandler;
        _transactionManager = transactionManager;
    }

    public async Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await SearchAsync(request, transaction, cancellationToken);
    }

    public async Task<List<TEntity>> SearchAsync(TSearch request, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var results = await PageAsync(request, entityTransaction, cancellationToken);
        return results?.Data?.ToList();
    }

    public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await PageAsync(request, transaction, cancellationToken);
    }

    public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        _transactionManager.AddTransaction(entityTransaction);
        Func<IMongoQueryable<TEntity>, Task<IMongoQueryable<TEntity>>> firstStageFilter = null;

        if (_searchHandler != null)
        {
            firstStageFilter = async x => (IMongoQueryable<TEntity>)_searchHandler.HandleSearch(x, request)
                                          ?? (IMongoQueryable<TEntity>)await _searchHandler.HandleSearchAsync(x, request);
        }

        var query = await _readClient.GetQueryableAsync(firstStageFilter, entityTransaction, cancellationToken);

        query = GetSearchExpressions(request).Aggregate(query, (current, expression) => current.Where(expression));

        var paged = await _readClient.GetPagedResponseAsync(query, request, cancellationToken);

        return paged;
    }
}
