using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Implementations
{
    public class EntityFrameworkEntityTransaction : BaseDisposable, IEntityTransaction
    {
        public IDbContextTransaction ContextTransaction { get;}

        public EntityFrameworkEntityTransaction(IDbContextTransaction contextTransaction)
        {
            ContextTransaction = contextTransaction;
        }

        public Task CompleteAsync(CancellationToken cancellationToken)
            => ContextTransaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken)
            => ContextTransaction.RollbackAsync(cancellationToken);
    }
}
