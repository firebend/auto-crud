using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IClientSessionHandle ClientSessionHandle { get; }

        public MongoEntityTransaction(IClientSessionHandle clientSessionHandle)
        {
            ClientSessionHandle = clientSessionHandle;
        }

        public Task CompleteAsync(CancellationToken cancellationToken)
            => ClientSessionHandle.CommitTransactionAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken)
            => ClientSessionHandle.AbortTransactionAsync(cancellationToken);

        protected override void DisposeManagedObjects() => ClientSessionHandle?.Dispose();
    }
}
