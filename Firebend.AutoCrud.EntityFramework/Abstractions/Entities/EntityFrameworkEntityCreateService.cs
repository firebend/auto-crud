using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntityCreateService<TKey, TEntity> : BaseDisposable, IEntityCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityFrameworkCreateClient<TKey, TEntity> _createClient;
        private readonly ISessionTransactionManager _transactionManager;

        protected EntityFrameworkEntityCreateService(IEntityFrameworkCreateClient<TKey, TEntity> createClient, ISessionTransactionManager transactionManager)
        {
            _createClient = createClient;
            _transactionManager = transactionManager;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
            return await _createClient.AddAsync(entity, transaction, cancellationToken);
        }

        public Task<TEntity> CreateAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken = default)
        {
            _transactionManager.AddTransaction(transaction);
            return _createClient.AddAsync(entity, transaction, cancellationToken);
        }

        protected override void DisposeManagedObjects() => _createClient?.Dispose();
    }
}
