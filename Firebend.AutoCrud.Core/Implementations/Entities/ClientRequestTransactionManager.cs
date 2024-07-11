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
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

public class ClientRequestTransactionManager : ISessionTransactionManager
{
    private record QueuedTransaction(IEntityTransaction Transaction, DateTimeOffset StartedDate)
    {
        public bool Removed { get; set; }
    }

    private readonly string _sessionId = Guid.NewGuid().ToString();
    private readonly ILogger<ClientRequestTransactionManager> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<string, Lazy<Task<IEntityTransaction>>> _sharedTransactions = new();
    private readonly ConcurrentBag<QueuedTransaction> _transactions = [];

    public bool TransactionStarted { get; private set; }
    public ImmutableList<Guid> TransactionIds => GetTransactionsInOrder(true).Select(x => x.Id).ToImmutableList();

    public ClientRequestTransactionManager(ILogger<ClientRequestTransactionManager> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void Start()
    {
        _logger.LogDebug("{SessionId}: Starting session transaction", _sessionId);
        TransactionStarted = true;
    }

    private IEnumerable<IEntityTransaction> GetTransactionsInOrder(bool ascending)
    {
        var activeTransactions = _transactions.Where(x => !x.Removed);
        var queuedTransactionsOrdered = ascending
            ? activeTransactions.OrderBy(x => x.StartedDate)
            : activeTransactions.OrderByDescending(x => x.StartedDate);
        return queuedTransactionsOrdered.Select(x => x.Transaction);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{SessionId}: Completing {TransactionIdsCount} transactions. {Join}", _sessionId, TransactionIds.Count, string.Join(',', TransactionIds));
        }

        foreach (var transaction in GetTransactionsInOrder(true))
        {
            await EntityTransactionMediator.TryCompleteAsync(transaction, cancellationToken);
        }

        ClearTransactions();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("{SessionId}: Rolling back {TransactionCount} transactions", _sessionId, TransactionIds.Count);
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

        var transactionFactory = _serviceProvider.GetRequiredService<IEntityTransactionFactory<TKey, TEntity>>();
        var key = await transactionFactory.GetDbContextHashCode();

        var transaction = await GetOrAddSharedTransaction(key, transactionFactory, cancellationToken);

        if (transactionFactory.ValidateTransaction(transaction))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("{SessionId}: Transaction for {Name} - {TransactionId} is valid",
                    _sessionId,
                    typeof(TEntity).Name,
                    transaction.Id);
            }

            return transaction;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{SessionId}: Transaction for {Name} - {TransactionId} is invalid",
                _sessionId,
                typeof(TEntity).Name,
                transaction.Id);
        }

        RemoveTransaction(key, transaction.Id);

        return await GetOrAddSharedTransaction(key, transactionFactory, cancellationToken);
    }

    private async Task<IEntityTransaction> GetOrAddSharedTransaction<TKey, TEntity>(string key,
        IEntityTransactionFactory<TKey, TEntity> transactionFactory, CancellationToken cancellationToken)
        where TKey : struct where TEntity : IEntity<TKey>
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{SessionId}: Getting or adding shared transaction for {Name} using key {Key}",
                _sessionId,
                typeof(TEntity).Name,
                key);
        }

        var transactionLazy = _sharedTransactions.GetOrAdd(key, (_, arg) =>
        {
            var (self, tf, ct) = arg;
            // Marcus - GetOrAdd is not thread safe, so we need to use a Lazy to ensure only one transaction is created
            var addTransactionLazy = new Lazy<Task<IEntityTransaction>>(async () =>
            {
                var transaction = await tf.StartTransactionAsync(ct);
                self.AddTransactionInternal(transaction);
                return transaction;
            });
            return addTransactionLazy;
        }, (this, transactionFactory, cancellationToken));

        return await transactionLazy.Value;
    }

    public void AddTransaction(IEntityTransaction transaction)
    {
        if (!CanAddTransaction(transaction))
        {
            return;
        }

        _logger.LogDebug("{SessionId}: Adding external transaction {TransactionId} to session", _sessionId,
            transaction.Id);
        AddTransactionInternal(transaction);
    }

    private bool CanAddTransaction(IEntityTransaction transaction) =>
        TransactionStarted && transaction != null && transaction.Id != Guid.Empty;

    private void AddTransactionInternal(IEntityTransaction transaction)
    {
        if (!CanAddTransaction(transaction))
        {
            return;
        }

        var existingTransaction = _transactions.Any(t => t.Transaction.Id == transaction.Id);

        if (existingTransaction)
        {
            return;
        }

        _logger.LogDebug("{SessionId}: Adding transaction {TransactionId} to session", _sessionId, transaction.Id);
        _transactions.Add(new QueuedTransaction(transaction, transaction.StartedDate));
    }

    private void RemoveTransaction(string key, Guid transactionId)
    {
        _logger.LogDebug("{SessionId}: Removing transaction {TransactionId} from session", _sessionId, transactionId);
        _sharedTransactions.TryRemove(key, out _);
        var queuedTransaction = _transactions.FirstOrDefault(t => t.Transaction.Id == transactionId);

        if (queuedTransaction is null)
        {
            return;
        }

        // Marcus - We don't want to dispose of the transaction just yet, just mark it as removed
        queuedTransaction.Removed = true;
    }

    private void ClearTransactions()
    {
        _logger.LogDebug("{SessionId}: Clearing transactions and stopping session", _sessionId);
        while (_transactions.TryTake(out var transaction))
        {
            transaction.Transaction.Dispose();
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
