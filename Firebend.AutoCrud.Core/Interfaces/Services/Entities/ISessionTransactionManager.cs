using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

/// <summary>
/// Used for associating all db requests in a transaction for a given session
/// </summary>
public interface ISessionTransactionManager
{
    /// <summary>
    /// True if session transaction has started
    /// </summary>
    bool TransactionStarted { get; }

    /// <summary>
    /// List of transaction ids associated to session transaction
    /// </summary>
    ImmutableList<Guid> TransactionIds { get; }
    /// <summary>
    /// Call to start tracking db requests in a transaction for the current session
    /// </summary>
    void Start();

    /// <summary>
    /// Calls CompleteAsync on all transactions registered in this session and then disposes of transactions
    /// </summary>
    Task CompleteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Calls RollbackAsync on all transactions registered in this session and then disposes of transactions
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RollbackAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns an IEntityTransaction if TransactionStarted is true; otherwise, returns null
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns>IEntityTransaction | null</returns>
    Task<IEntityTransaction> GetTransaction<TKey, TEntity>(CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : IEntity<TKey>;

    /// <summary>
    /// Adds a transaction to track if it is not already being tracked by the session
    /// </summary>
    /// <param name="transaction"></param>
    void AddTransaction(IEntityTransaction transaction);
}
