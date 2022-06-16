using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface ISessionTransactionManager
{
    bool TransactionStarted { get; }
    void Start();
    Task Commit(CancellationToken cancellationToken);
    Task Rollback(CancellationToken cancellationToken);
    Task<IEntityTransaction> GetTransaction<TKey, TEntity>(CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : IEntity<TKey>;
}
