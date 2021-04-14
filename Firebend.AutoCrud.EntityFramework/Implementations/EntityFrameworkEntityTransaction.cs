using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Implementations
{
    public class EntityFrameworkEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IDbContextTransaction ContextTransaction { get; }

        public EntityFrameworkEntityTransaction(IDbContextTransaction contextTransaction, IEntityTransactionOutbox outbox)
        {
            ContextTransaction = contextTransaction;
            Outbox = outbox;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public async Task CompleteAsync(CancellationToken cancellationToken)
        {
            await ContextTransaction.CommitAsync(cancellationToken);
            await Outbox.InvokeEnrollmentsAsync(Id, cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await ContextTransaction.RollbackAsync(cancellationToken);
            await Outbox.ClearEnrollmentsAsync(Id, cancellationToken);
        }

        public IEntityTransactionOutbox Outbox { get; }
    }
}

