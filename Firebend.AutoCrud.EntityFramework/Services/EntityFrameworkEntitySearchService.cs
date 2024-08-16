using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Services;

public class EntityFrameworkEntitySearchService<TKey, TEntity, TSearch> :
    AbstractEntitySearchService<TEntity, TSearch>,
    IEntitySearchService<TKey, TEntity, TSearch>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TSearch : IEntitySearchRequest
{
    private readonly IEntityFrameworkQueryClient<TKey, TEntity> _searchClient;
    private readonly ISessionTransactionManager _transactionManager;
    private readonly IEntitySearchHandler<TKey, TEntity, TSearch> _searchHandler;

    public EntityFrameworkEntitySearchService(IEntityFrameworkQueryClient<TKey, TEntity> searchClient,
        ISessionTransactionManager transactionManager,
        IEntitySearchHandler<TKey, TEntity, TSearch> searchHandler = null)
    {
        _searchClient = searchClient;
        _transactionManager = transactionManager;
        _searchHandler = searchHandler;
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
        request.DoCount = false;

        var results = await PageAsync(request, entityTransaction, cancellationToken);

        return results?.Data?.ToList();
    }

    public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await PageAsync(request, transaction, cancellationToken);
    }

    public async Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var (query, context) = await _searchClient
            .GetQueryableAsync(true, entityTransaction, cancellationToken);

        await using (context)
        {
            query = GetSearchExpressions(request).Aggregate(query, (current, expression) => current.Where(expression));

            if (_searchHandler != null)
            {
                query = _searchHandler.HandleSearch(query, request)
                    ?? await _searchHandler.HandleSearchAsync(query, request);
            }

            var paged = await _searchClient.GetPagedResponseAsync(query, request, true, cancellationToken);

            return paged;
        }
    }

    protected override void DisposeManagedObjects() => _searchClient?.Dispose();
}
