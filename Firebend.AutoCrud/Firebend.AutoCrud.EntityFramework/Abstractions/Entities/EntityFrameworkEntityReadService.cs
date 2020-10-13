using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public class EntityFrameworkEntityReadService<TKey, TEntity> : IEntityReadService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityFrameworkQueryClient<TKey, TEntity> _readClient;

        public EntityFrameworkEntityReadService(IEntityFrameworkQueryClient<TKey, TEntity> readClient)
        {
            _readClient = readClient;
        }

        public Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return _readClient.GetByKeyAsync(key, cancellationToken);
        }

        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _readClient.GetAllAsync(cancellationToken);
        }
    }
}