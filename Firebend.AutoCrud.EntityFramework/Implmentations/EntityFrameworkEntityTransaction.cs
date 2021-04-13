using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Implmentations
{
    public class EntityFrameworkEntityTransaction : BaseDisposable, IEntityTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private readonly IDbContext _dbContext;

        public EntityFrameworkEntityTransaction(IDbContextTransaction transaction, IDbContext dbContext)
        {
            _transaction = transaction;
            _dbContext = dbContext;
        }

        public Task CompleteAsync(CancellationToken cancellationToken)
        => _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken)
            => _transaction.RollbackAsync(cancellationToken);
    }
}
