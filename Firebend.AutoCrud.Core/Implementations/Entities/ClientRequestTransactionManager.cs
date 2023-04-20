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
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ConcurrentDictionary<string, Task<IEntityTransaction>> _sharedTransactions = new();
    private readonly ConcurrentQueue<IEntityTransaction> _transactions = new();

    public bool TransactionStarted { get; private set; }
    public ImmutableList<Guid> TransactionIds => _transactions.Select(x => x.Id).ToImmutableList();

    public ClientRequestTransactionManager(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Start() => TransactionStarted = true;

    private IEnumerable<IEntityTransaction> GetTransactionsInOrder(bool ascending)
    {
        IEnumerable<IEntityTransaction> EntityTransactions()
        {
            while (_transactions.TryDequeue(out var t))
            {
                yield return t;
            }
        }

        if (ascending)
        {
            return EntityTransactions().OrderBy(x => x.StartedDate).ToArray();
        }

        return EntityTransactions().OrderByDescending(x => x.StartedDate).ToArray();
    }

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        foreach (var transaction in GetTransactionsInOrder(true))
        {
            await EntityTransactionMediator.TryCompleteAsync(transaction, cancellationToken);
        }

        ClearTransactions();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        foreach (var transaction in GetTransactionsInOrder(false))
        {
            await EntityTransactionMediator.TryRollbackAsync(transaction, cancellationToken);
        }

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

        using var scope = _scopeFactory.CreateScope();
        var transactionFactory = scope.ServiceProvider.GetRequiredService<IEntityTransactionFactory<TKey, TEntity>>();
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

        _transactions.Enqueue(transaction);
    }

    private void ClearTransactions()
    {
        while (_transactions.TryDequeue(out var transaction))
        {
            transaction.Dispose();
        }

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
