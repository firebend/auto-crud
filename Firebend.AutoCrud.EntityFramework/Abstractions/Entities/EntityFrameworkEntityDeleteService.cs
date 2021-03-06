using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntityDeleteService<TKey, TEntity> : BaseDisposable, IEntityDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityFrameworkDeleteClient<TKey, TEntity> _deleteClient;

        protected EntityFrameworkEntityDeleteService(IEntityFrameworkDeleteClient<TKey, TEntity> deleteClient)
        {
            _deleteClient = deleteClient;
        }

        public Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
            => _deleteClient.DeleteAsync(key, cancellationToken);

        public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
            => _deleteClient.DeleteAsync(key, entityTransaction, cancellationToken);

        protected override void DisposeManagedObjects() => _deleteClient?.Dispose();
    }
}
