using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Models
{
    public enum EntityTransactionState
    {
        Started = 0,
        Completed = 1,
        RolledBack = 2
    }

    public static class EntityTransactionMediator
    {
        private static readonly AsyncKeyedLocker<Guid> Locker = new();

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

    public interface IEntityTransaction : IDisposable
    {
        Guid Id { get; }

        Task CompleteAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);

        IEntityTransactionOutbox Outbox { get; }

        public EntityTransactionState State { get; set; }

        public DateTimeOffset StartedDate { get; set; }
    }

    public static class EntityTransactionExtensions
    {
        public static Task AddFunctionEnrollmentAsync(this IEntityTransaction source,
            Func<CancellationToken, Task> func,
            CancellationToken cancellationToken)
            => source.Outbox.AddEnrollmentAsync(source.Id.ToString(), new FunctionTransactionOutboxEnrollment(func), cancellationToken);
    }
}
