using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities;

public abstract class MongoEntityUpdateService<TKey, TEntity> : BaseDisposable, IEntityUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IMongoUpdateClient<TKey, TEntity> _updateClient;
    private readonly ISessionTransactionManager _transactionManager;

    protected MongoEntityUpdateService(IMongoUpdateClient<TKey, TEntity> updateClient,
        ISessionTransactionManager transactionManager)
    {
        _updateClient = updateClient;
        _transactionManager = transactionManager;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);

        // Allow creating entities through PUT to make it easier to set the guid in the client
        // when creating new entities. ( ACID2.0 )
        return await _updateClient.UpsertAsync(entity, transaction, cancellationToken);
    }

    public Task<TEntity> UpdateAsync(TEntity entity,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _updateClient.UpsertAsync(entity, entityTransaction, cancellationToken);
    }

    public async Task<TEntity> PatchAsync(TKey key,
        JsonPatchDocument<TEntity> jsonPatchDocument,
        CancellationToken cancellationToken = default)
    {
        if (key.Equals(default))
        {
            throw new ArgumentException("Key is invalid", nameof(key));
        }

        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _updateClient.UpdateAsync(key, jsonPatchDocument, transaction, cancellationToken);
    }

    public Task<TEntity> PatchAsync(TKey key,
        JsonPatchDocument<TEntity> jsonPatchDocument,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _updateClient.UpdateAsync(key, jsonPatchDocument, entityTransaction, cancellationToken);
    }
}
