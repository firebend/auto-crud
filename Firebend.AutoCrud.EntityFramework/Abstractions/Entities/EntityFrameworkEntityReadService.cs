using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntityReadService<TKey, TEntity> : BaseDisposable, IEntityReadService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityFrameworkQueryClient<TKey, TEntity> _readClient;

        protected EntityFrameworkEntityReadService(IEntityFrameworkQueryClient<TKey, TEntity> readClient)
        {
            _readClient = readClient;
        }

        public Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default)
            => _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), false, cancellationToken);

        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => _readClient.GetAllAsync(null, cancellationToken);

        protected override void DisposeManagedObjects() => _readClient?.Dispose();
    }
}
