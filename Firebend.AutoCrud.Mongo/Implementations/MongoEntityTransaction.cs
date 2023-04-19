using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Firebend.AutoCrud.Core.Ids;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public static class MongoEntityTransactionsDefaults
    {
        public static int NumberOfRetries = 10;
    }

    public class MongoEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IClientSessionHandle ClientSessionHandle { get; }
        public IEntityTransactionOutbox Outbox { get; }
        public EntityTransactionState State { get; set; }
        public DateTimeOffset StartedDate { get; set; }

        private readonly IMongoRetryService _retry;
        private readonly AsyncKeyedLocker<Guid> _locker = new();

        public MongoEntityTransaction(IClientSessionHandle clientSessionHandle,
            IEntityTransactionOutbox outbox,
            IMongoRetryService retry)
        {
            ClientSessionHandle = clientSessionHandle;
            Outbox = outbox;
            _retry = retry;
            Id = CombGuid.New();
            State = EntityTransactionState.Started;
            StartedDate = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; }

        private async Task<bool> TryCommitTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ClientSessionHandle.CommitTransactionAsync(cancellationToken);
            }
            catch (MongoCommandException ex)
            {
                //********************************************
                // Author: JMA
                // Date: 2023-04-19 06:49:36
                // Comment: Transaction has been aborted error code
                //*******************************************
                if (ex.Code == 251)
                {
                    return false;
                }
                throw;
            }

            return true;
        }

        private bool TryGetMongoTransactionState(out CoreTransactionState? state)
        {
            state = null;

            try
            {
                if (ClientSessionHandle?.WrappedCoreSession is WrappingCoreSession session)
                {
                    state = session.CurrentTransaction.State;
                    return true;
                }
            }
            catch (ObjectDisposedException)
            {
                return false;
            }

            return false;
        }

        private bool CanActOnTransaction(bool isRollback)
        {
            if (State != EntityTransactionState.Started)
            {
                return false;
            }

            if (!TryGetMongoTransactionState(out var state))
            {
                return false;
            }

            var canAct = isRollback
             ? state is CoreTransactionState.Starting or CoreTransactionState.InProgress
             : state is CoreTransactionState.Starting or CoreTransactionState.InProgress or CoreTransactionState.Committed;

            if (canAct is false)
            {
                Console.WriteLine($"**************** can act is false {state} {(isRollback ? "rollback" : "commit")}");
            }

            return canAct;
        }

        public async Task CompleteAsync(CancellationToken cancellationToken)
        {
            using var locked = await _locker.LockAsync(Id, cancellationToken);

            if (CanActOnTransaction(false) is false)
            {
                return;
            }

            var wasCompleted = await _retry.RetryErrorAsync(async () =>
            {
                if (CanActOnTransaction(false) is false)
                {
                    return false;
                }

                return await TryCommitTransactionAsync(cancellationToken);
            }, MongoEntityTransactionsDefaults.NumberOfRetries);

            if (wasCompleted)
            {
                await Outbox.InvokeEnrollmentsAsync(Id.ToString(), cancellationToken);
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            using var locked = await _locker.LockAsync(Id, cancellationToken);

            if (CanActOnTransaction(true) is false)
            {
                return;
            }

            var wasRolledBack = await _retry.RetryErrorAsync(async () =>
            {
                if (CanActOnTransaction(true) is false)
                {
                    return false;
                }

                await ClientSessionHandle.AbortTransactionAsync(cancellationToken);
                return true;
            }, MongoEntityTransactionsDefaults.NumberOfRetries);

            if (wasRolledBack)
            {
                await Outbox.ClearEnrollmentsAsync(Id.ToString(), cancellationToken);
            }
        }

        protected override void DisposeManagedObjects() => ClientSessionHandle?.Dispose();
    }
}
