using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public static class MongoEntityTransactionsDefaults
    {
        public static int NumberOfRetries = 10;
    }

    public class MongoEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IClientSessionHandle ClientSessionHandle { get; }

        private readonly IMongoRetryService _retry;

        public MongoEntityTransaction(IClientSessionHandle clientSessionHandle,
            IEntityTransactionOutbox outbox,
            IMongoRetryService retry)
        {
            ClientSessionHandle = clientSessionHandle;
            Outbox = outbox;
            _retry = retry;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public async Task CompleteAsync(CancellationToken cancellationToken)
        {
            await _retry.RetryErrorAsync(async () =>
            {
                await ClientSessionHandle.CommitTransactionAsync(cancellationToken);
                return true;
            }, MongoEntityTransactionsDefaults.NumberOfRetries);

            await Outbox.InvokeEnrollmentsAsync(Id.ToString(), cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await _retry.RetryErrorAsync(async () =>
            {
                await ClientSessionHandle.AbortTransactionAsync(cancellationToken);
                return true;
            }, MongoEntityTransactionsDefaults.NumberOfRetries);

            await Outbox.ClearEnrollmentsAsync(Id.ToString(), cancellationToken);
        }

        public IEntityTransactionOutbox Outbox { get; }

        protected override void DisposeManagedObjects() => ClientSessionHandle?.Dispose();
    }
}
