using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Implementations
{
    public class EntityFrameworkEntityTransactionFactory<TKey, TEntity> : IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IDbContextProvider<TKey, TEntity> _dbContextProvider;
        private readonly IEntityTransactionOutbox _outbox;

        public EntityFrameworkEntityTransactionFactory(IDbContextProvider<TKey, TEntity> dbContextProvider, IEntityTransactionOutbox outbox)
        {
            _dbContextProvider = dbContextProvider;
            _outbox = outbox;
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var context = await _dbContextProvider.GetDbContextAsync(cancellationToken);
            var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            return new EntityFrameworkEntityTransaction(transaction, _outbox);
        }
    }
}
