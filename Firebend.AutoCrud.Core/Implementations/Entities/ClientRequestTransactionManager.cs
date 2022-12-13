using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

public class ClientRequestTransactionManager : ISessionTransactionManager
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<string, Task<IEntityTransaction>> _sharedTransactions = new();
    private readonly ConcurrentBag<IEntityTransaction> _transactions = new();
    public bool TransactionStarted { get; private set; }
    public ImmutableList<Guid> TransactionIds => _transactions.Select(x => x.Id).ToImmutableList();

    public ClientRequestTransactionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Start() => TransactionStarted = true;

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_transactions.Select(x => x.CompleteAsync(cancellationToken)));
        ClearTransactions();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_transactions.Select(x => x.RollbackAsync(cancellationToken)));
        ClearTransactions();
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
        var key = await transactionFactory.GetDbContextHashCode();

        var transaction = await GetOrAddSharedTransaction(key, transactionFactory, cancellationToken);

        if (transactionFactory.ValidateTransaction(transaction))
        {
            return transaction;
        }

        transaction.Dispose();
        _sharedTransactions.Remove(key, out _);
        return await GetOrAddSharedTransaction(key, transactionFactory, cancellationToken);
    }

    private async Task<IEntityTransaction> GetOrAddSharedTransaction<TKey, TEntity>(string key,
        IEntityTransactionFactory<TKey, TEntity> transactionFactory, CancellationToken cancellationToken)
        where TKey : struct where TEntity : IEntity<TKey>
    {
        var transaction = await _sharedTransactions.GetOrAdd(key, static async (_, arg) =>
            {
                var (self, tf, ct) = arg;
                var transaction = await tf.StartTransactionAsync(ct);
                self.AddTransaction(transaction);
                return transaction;
            }, (this, transactionFactory, cancellationToken));
        return transaction;
    }

    public void AddTransaction(IEntityTransaction transaction)
    {
        if (!TransactionStarted || transaction is null || transaction.Id == Guid.Empty)
        {
            return;
        }

        var existingTransaction = _transactions.Any(t => t.Id == transaction.Id);
        if (existingTransaction)
        {
            return;
        }

        _transactions.Add(transaction);
    }

    private void ClearTransactions()
    {
        _transactions.ToList().ForEach(x => x.Dispose());
        _sharedTransactions.Clear();
        _transactions.Clear();
        TransactionStarted = false;
    }

    public void Dispose()
    {
        ClearTransactions();
        GC.SuppressFinalize(this);
    }
}
