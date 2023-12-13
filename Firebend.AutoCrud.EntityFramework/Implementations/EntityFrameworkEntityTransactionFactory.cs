using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Implementations
{
    public class EntityFrameworkEntityTransactionFactory<TKey, TEntity> : IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IDbContextProvider<TKey, TEntity> _dbContextProvider;
        private readonly IEntityTransactionOutbox _outbox;
        private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;

        public EntityFrameworkEntityTransactionFactory(IDbContextProvider<TKey, TEntity> dbContextProvider,
            IEntityTransactionOutbox outbox, IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider)
        {
            _dbContextProvider = dbContextProvider;
            _outbox = outbox;
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<string> GetDbContextHashCode()
        {
            var connectionString = await _connectionStringProvider.GetConnectionStringAsync();
            var hashCode = connectionString.GetHashCode();
            return $"ef_{hashCode}";
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var context = await _dbContextProvider.GetDbContextAsync(cancellationToken);
            var transaction =
                await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            return new EntityFrameworkEntityTransaction(transaction, _outbox);
        }

        public bool ValidateTransaction(IEntityTransaction transaction)
        {
            if (transaction is not EntityFrameworkEntityTransaction efTransaction)
            {
                return false;
            }

            var dbTransaction = efTransaction.ContextTransaction.GetDbTransaction();
            return dbTransaction.Connection is not null && dbTransaction.Connection.State == ConnectionState.Open;
        }
    }
}
