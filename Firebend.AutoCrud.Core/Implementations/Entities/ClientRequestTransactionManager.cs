using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

public class ClientRequestTransactionManager : ISessionTransactionManager, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<Type, Task<IEntityTransaction>> _sharedTransactions = new();
    private readonly ConcurrentBag<IEntityTransaction> _transactions = new();
    public bool TransactionStarted { get; private set; }

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
        return await _sharedTransactions.GetOrAdd(transactionFactory.GetType(),
            async (_) =>
            {
                var transaction = await transactionFactory.StartTransactionAsync(cancellationToken);
                _transactions.Add(transaction);
                return transaction;
            });
    }

    public void AddTransaction(IEntityTransaction transaction)
    {
        if (transaction is null || transaction.Id == Guid.Empty)
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

    public void Dispose() => ClearTransactions();
}
