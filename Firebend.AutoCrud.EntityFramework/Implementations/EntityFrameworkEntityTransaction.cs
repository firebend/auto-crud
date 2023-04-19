using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Ids;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Implementations
{
    public class EntityFrameworkEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IDbContextTransaction ContextTransaction { get; }
        public IEntityTransactionOutbox Outbox { get; }
        public EntityTransactionState State { get; set; }
        public DateTimeOffset StartedDate { get; set; }

        public EntityFrameworkEntityTransaction(IDbContextTransaction contextTransaction, IEntityTransactionOutbox outbox)
        {
            ContextTransaction = contextTransaction;
            Outbox = outbox;
            Id = CombGuid.New();
            State = EntityTransactionState.Started;
            StartedDate = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; }

        public async Task CompleteAsync(CancellationToken cancellationToken)
        {
            if (State != EntityTransactionState.Started)
            {
                return;
            }

            await ContextTransaction.CommitAsync(cancellationToken);
            await Outbox.InvokeEnrollmentsAsync(Id.ToString(), cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (State != EntityTransactionState.Started)
            {
                return;
            }

            await ContextTransaction.RollbackAsync(cancellationToken);
            await Outbox.ClearEnrollmentsAsync(Id.ToString(), cancellationToken);
        }

        protected override void DisposeManagedObjects() => ContextTransaction?.Dispose();
    }
}
