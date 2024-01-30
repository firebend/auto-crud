using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public static class EntityTransactionMediator
{
    private static readonly AsyncKeyedLocker<Guid> Locker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    private static async Task<bool> TryToggleStateAsync(IEntityTransaction transaction,
        EntityTransactionState desiredState,
        CancellationToken cancellationToken)
    {
        using var locked = await Locker.LockAsync(transaction.Id, cancellationToken);

        if (transaction.State != EntityTransactionState.Started)
        {
            return false;
        }

        switch (desiredState)
        {
            case EntityTransactionState.Completed:
                await transaction.CompleteAsync(cancellationToken);
                transaction.State = EntityTransactionState.Completed;
                return true;
            case EntityTransactionState.RolledBack:
                await transaction.RollbackAsync(cancellationToken);
                transaction.State = EntityTransactionState.RolledBack;
                return true;
            case EntityTransactionState.Started:
            default:
                throw new ArgumentOutOfRangeException(nameof(desiredState), desiredState, null);
        }
    }

    public static Task<bool> TryCompleteAsync(IEntityTransaction transaction, CancellationToken cancellationToken)
        => TryToggleStateAsync(transaction, EntityTransactionState.Completed, cancellationToken);

    public static Task<bool> TryRollbackAsync(IEntityTransaction transaction, CancellationToken cancellationToken)
        => TryToggleStateAsync(transaction, EntityTransactionState.RolledBack, cancellationToken);
}
