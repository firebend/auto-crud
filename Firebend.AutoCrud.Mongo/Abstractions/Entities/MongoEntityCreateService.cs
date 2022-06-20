using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Entities;

public abstract class MongoEntityCreateService<TKey, TEntity> : BaseDisposable, IEntityCreateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IMongoCreateClient<TKey, TEntity> _createClient;
    private readonly ISessionTransactionManager _transactionManager;

    protected MongoEntityCreateService(IMongoCreateClient<TKey, TEntity> createClient,
        ISessionTransactionManager transactionManager)
    {
        _createClient = createClient;
        _transactionManager = transactionManager;
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _createClient.CreateAsync(entity, transaction, cancellationToken);
    }

    public Task<TEntity> CreateAsync(TEntity entity, IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(transaction);
        return _createClient.CreateAsync(entity, transaction, cancellationToken);
    }
}
