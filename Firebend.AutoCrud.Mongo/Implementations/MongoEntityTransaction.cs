using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransaction : BaseDisposable, IEntityTransaction
    {
        private readonly IClientSessionHandle _clientSessionHandle;

        public MongoEntityTransaction(IClientSessionHandle clientSessionHandle)
        {
            _clientSessionHandle = clientSessionHandle;
        }

        public Task CompleteAsync(CancellationToken cancellationToken)
            => _clientSessionHandle.CommitTransactionAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken)
            => _clientSessionHandle.AbortTransactionAsync(cancellationToken);

        protected override void DisposeManagedObjects() => _clientSessionHandle?.Dispose();
    }
}
