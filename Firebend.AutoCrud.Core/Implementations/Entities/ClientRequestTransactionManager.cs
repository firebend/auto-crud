using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

public class ClientRequestTransactionManager : ISessionTransactionManager, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<Type, Task<IEntityTransaction>> _transactions = new();
    public bool TransactionStarted { get; private set; }

    public ClientRequestTransactionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Start() => TransactionStarted = true;

    public async Task Commit(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_transactions.Values.Select(async x => (await x).CompleteAsync(cancellationToken)));
        await ClearTransactions();
    }

    public async Task Rollback(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_transactions.Values.Select(async x => (await x).RollbackAsync(cancellationToken)));
        await ClearTransactions();
    }

    public async Task<IEntityTransaction> GetTransaction<TKey, TEntity>(CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        if (!TransactionStarted)
        {
            return null;
        }

        var transactionFactory = _serviceProvider.GetRequiredService<IEntityTransactionFactory<TKey, TEntity>>();
        return await _transactions.GetOrAdd(transactionFactory.GetType(),
            Task(_) => transactionFactory.StartTransactionAsync(cancellationToken));
    }

    private async Task ClearTransactions()
    {
        await Task.WhenAll(_transactions.Values.Select(async x => (await x).Dispose()));
        _transactions.Clear();
        TransactionStarted = false;
    }

    public async ValueTask DisposeAsync() => await ClearTransactions();
}
