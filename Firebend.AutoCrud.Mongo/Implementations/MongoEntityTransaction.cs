using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IClientSessionHandle ClientSessionHandle { get; }

        public MongoEntityTransaction(IClientSessionHandle clientSessionHandle, IEntityTransactionOutbox outbox)
        {
            ClientSessionHandle = clientSessionHandle;
            Outbox = outbox;
            Id = new Guid();
        }

        public Guid Id { get; }

        public async Task CompleteAsync(CancellationToken cancellationToken)
        {
            await ClientSessionHandle.CommitTransactionAsync(cancellationToken);
            await Outbox.InvokeEnrollmentsAsync(Id, cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await ClientSessionHandle.AbortTransactionAsync(cancellationToken);
            await Outbox.ClearEnrollmentsAsync(Id, cancellationToken);
        }

        public IEntityTransactionOutbox Outbox { get; }

        protected override void DisposeManagedObjects() => ClientSessionHandle?.Dispose();
    }
}
